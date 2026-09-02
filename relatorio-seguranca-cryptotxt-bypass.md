# Relatório de Segurança — CryptoTxt v1.4 — Bypass de Senha (Login)

- **Data:** 12/08/2026
- **Alvo:** `C:\Projetos\exe.analise\CryptoTxt\CryptoTxt.exe` (hash PBKDF2 embutido)
- **Artefato do teste:** `CryptoTxt-nopass.exe` (bundle patcheado, senha removida)
- **Tipo:** Engenharia reversa de aplicação desktop .NET self-contained; escalada de privilégio não se aplica
- **Resultado:** A autenticação de login foi completamente neutralizada em ~30 minutos, sem conhecimento prévio da senha. A chave criptográfica padrão usada para cifrar arquivos `.txt` também é recuperável do binário.

---

## 1. Resumo Executivo

O aplicativo **CryptoTxt.exe** é um binário **.NET self-contained single-file** que contém, embutido e
decompilável, todo o código gerenciado: a lógica de login, a configuração de credenciais (`login.txt`) e a
chave/IV AES padrão usada para criptografar os arquivos de texto.

Três fatos combinados tornam o aplicativo vulnerável:

1. **A decisão de autenticação acontece 100% no cliente** — basta alterar um método do IL para que qualquer
   usuário/senha seja aceito.
2. **O binário não tem proteção contra adulteração (anti-tamper), ofuscação ou assinatura** — a alteração do
   IL foi feita e o executável continuou funcionando normalmente.
3. **A chave criptográfica "padrão" é a mesma para todas as instalações e está gravada em claro no código** —
   quem recupera o binário consegue descriptografar qualquer arquivo `.txt.enc` produzido com essa chave, sem
   senha.

**Impacto:** perda de confidencialidade dos arquivos criptografados com a chave padrão e total neutralização do
controle de acesso (login).

**Recomendação principal:** mover a autenticação para um serviço remoto (ou, se for um produto offline, tratar o
login como "proteção cosmética") **e** eliminar a chave padrão embutida, substituindo por chaves derivadas de
segredo do usuário protegidas por DPAPI/TPM.

---

## 2. Escopo e Metodologia

Técnicas utilizadas:

1. **Extração do bundle single-file** (.NET bundle format v6): localização da assinatura de 32 bytes no apphost,
   leitura do `header_offset`, parse do manifest (254 arquivos) e descompressão (Deflate) do `CryptoTxt.dll`.
2. **Decompilação** do assembly com `ilspycmd` (ILSpy) — recuperação fiel do código-fonte.
3. **Patch de IL** com dnlib: substituição do corpo do método `ValidateCredentials` por `ldc.i4.1; ret`.
4. **Reempacotamento** do bundle v6 com o assembly modificado.
5. **Validação funcional** por automação de UI: credenciais inválidas + Enter abriram o form principal.

> Python/ferramentas nativas não foram necessárias. Todo o fluxo foi executado com tooling público de .NET
> (SDK, ilspycmd, dnlib), disponível gratuitamente.

---

## 3. Visão Geral do Alvo (quem é o binário)

| Item | Valor encontrado |
|---|---|
| Formato | PE32+ (AMD64), originalmente publicado como `CryptoTxt.dll` |
| Empacotamento | Self-contained single-file (.NET bundle v6, 254 arquivos, compressão Deflate) |
| Framework | .NET (CoreCLR embutido — `coreclr`, `hostpolicy`, `clrjit` presentes no bundle) |
| UI | WinForms |
| Assinatura Authenticode | **Não assinado** (original e patcheado) |
| Credenciais embutidas | `login.txt`: `usuario:root`, salt `dFWt+s7nuoBp2LiTnEq2CQ==`, hash PBKDF2(SHA256, 310.000 iterações), dica `r...A` |

Fluxo de login (`CryptoTxt.LoginForm.btnLogin_Click`):

```
Validar credenciais (usuario + senha)
  └─ LoginConfiguration.ValidateCredentials(user, pass)
       1. string.Equals(user, UserName)                    → compara em claro
       2. PBKDF2-SHA256(pass, salt, 310000 iters) → 32 bytes
       3. CryptographicOperations.FixedTimeEquals(hash)     → true/false (local)
Se true → DialogResult.OK → MainForm
Se false→ "Usuário ou senha inválidos!" + lockout de 15s após 5 tentativas
```

---

## 4. Descobertas e Severidade

### V1 — Autenticação 100% no cliente (decisão de acesso no binário) — **CRÍTICA**

- **Onde:** `CryptoTxt/LoginForm.cs:110` + `CryptoTxt.Utils/LoginConfiguration.cs:22`
- **Descrição:** a checagem de senha é executada e decidida localmente. Qualquer alteração no IL do
  `ValidateCredentials` (colocar `ldc.i4.1` = retornar `true`) faz com que **qualquer** usuário/senha entre.
- **Prova:** após o patch, `usuario_invalido` / `senha_invalida_123` abriram o `MainForm`.
- **Consequência:** a senha deixa de ter valor de controle de acesso. Não é possível "fortalecer" a checagem
  local; o atacante sempre controla o binário.

### V2 — Assembly totalmente extraível e decompilável (sem ofuscação/anti-tamper) — **ALTA**

- **Onde:** todo o bundle; seções `.text/.rdata/.data` + payload.
- **Descrição:** o `CryptoTxt.dll` foi extraído em minutos. O IL está legível; a decompilação recuperou o
  código-fonte quase 1:1 (`Program.cs`, `LoginForm.cs`, `MainForm.cs`, `SharedCrypto.cs`, etc.).
- **Consequência:** o conhecimento interno do produto (algoritmos, chaves, lógica) torna-se público para quem
  possui o binário.

### V3 — Chave criptográfica padrão embutida e compartilhada entre instalações — **CRÍTICA**

- **Onde:** `CryptoCommon/SharedCrypto.cs:16` (`EmbeddedDefaultKey`, 32 bytes) e `:24` (`EmbeddedDefaultIV`, 16 bytes).
- **Descrição:** `GetOrCreateUserKeyMaterial()` devolve uma chave AES-256 e um IV fixos, iguais para todos os
  usuários e gravados em claro no código decompilável.
- **Consequência:** qualquer arquivo `.txt.enc` cifrado com a chave **padrão** (não importada/gerada) pode ser
  descriptografado por um atacante **sem a senha**, bastando extrair a chave do binário e rodar o
  `SharedCrypto.DecryptBytesWithKey`. A criptografia é forte (AES-GCM + HKDF-SHA256), mas a **segredo** não é —
  a chave não é um segredo.
- **Nota de design:** a existência de "Gerar Chave"/"Importar Chave" é correta, mas o *fallback silencioso* para
  a chave padrão (`GetActiveKeyMaterial`, em `CryptoUtils.cs:198`) é o ponto de exploração.

### V4 — Sem integridade / sem assinatura / sem auto-pin do código — **ALTA**

- **Onde:** processo de build e execução do exe (não há verificação própria de integridade).
- **Descrição:** o executável **não é assinado** (Authenticode: `NotSigned`) e o runtime não valida a integridade
  do bundle. Um bytecode modificado (como o IL do login) é carregado e executado sem qualquer aviso.
- **Consequência:** distribuição de binário adulterado (trojanização / redistribuição "sem senha") não é
  detectável.

### V5 — Anti-debug inofensivo (baseado apenas em `IsDebuggerPresent`) — **MÉDIA**

- **Onde:** `CryptoTxt/Program.cs:12-33`
- **Descrição:** usa `Debugger.IsAttached` + `IsDebuggerPresent()` no início e numa thread a cada 1s. São trunções
  nativas facilmente contornadas (patch do byte, limpeza do flag `BeingDebugged` no PEB, loader de prompt
  suspenso, etc.). Além disso, o assembly pode ser analisado **sem executar** (decompilação estática), então
  anti-debug não protege nada.
- **Consequência:** falso sentimento de proteção; apenas atrapalha um pouco a inspeção dinâmica.

### V6 — Lockout / anti-força-bruta apenas em memória do processo — **MÉDIA**

- **Onde:** `CryptoTxt/LoginForm.cs:85-101` (`failedAttempts`, `lockoutUntilUtc`)
- **Descrição:** o contador de falhas e o bloqueio de 15s vivem na memória do processo. Reiniciar o app,
  reprocessar o patch, ou simplesmente chamar `ValidateCredentials` fora da UI zera o bloqueio.
- **Consequência:** proteção contra força-bruta ineficaz contra automação offline/patcheada. Sem relevância
  quando V1 é explorada.

### V7 — Dica de senha exibida na tela de login — **BAIXA/MÉDIA**

- **Onde:** `CryptoTxt/LoginForm.cs:14,57-67`; `login.txt` (`dicadesenha:r...A`)
- **Descrição:** a dica reduz drasticamente o espaço de busca da senha e é exibida para qualquer pessoa.
- **Consequência:** facilita ataque offline de força-bruta ao hash PBKDF2 extraído (310k iterações atrasa, não
  impede).

---

## 5. Prova de Conceito (passos executados)

```text
1. dotnet tool install -g ilspycmd
2. Parse do bundle v6 do exe → extração de CryptoTxt.dll (Deflate)
3. ilspycmd -p CryptoTxt.dll  → código-fonte completo (login, chaves, algoritmo)
4. Patch IL com dnlib:
      LoginConfiguration.ValidateCredentials
         IL_0000: ldc.i4.1   (antes: comparação PBKDF2)
         IL_0001: ret
5. Reempacotamento do bundle v6 com o assembly modificado
      - recompressão Deflate do CryptoTxt.dll
      - recalculo de offsets/sizes/compressedSize do manifest
      - atualização do header_offset no apphost (placeholder antes da assinatura de 32 bytes)
6. Validação:
      CryptoTxt.exe    (oficial): senha errada → permanece no Login
      CryptoTxt-nopass.exe:       senha errada → abre o MainForm ("CryptoTxt - v1.4")
```

---

## 6. Análise de Causa Raiz

A causa raiz não é "hash fraco" (PBKDF2 com 310.000 iterações é boa prática) — é o **modelo de confiança**:

> **Regra de ouro da criptografia:** a chave (ou o teste de autenticação) não pode residir no mesmo binário
> que o atacante controla. Em aplicativos desktop, o usuário possui a máquina e o binário; qualquer segredo
> embutido ou teste local executável é, mais cedo ou mais tarde, extraível e contornável.

As vulnerabilidades V1..V7 são consequência desse modelo. Nenhuma quantidade de ofuscação, anti-debug ou
"bloqueio de 15s" muda o fato de que **a decisão final acontece em código que o atacante pode modificar**.

---

## 7. Recomendações (priorizadas)

### P0 — Autenticação (resolve V1 e V6)

1. **Mover a validação para um serviço remoto (online).**
   - O app envia `usuario + senha` (TLS) ou um desafio-assinado; a validação do PBKDF2/hash e o lockout ficam
     **server-side**.
   - O `DialogResult.OK` só é liberado após resposta autenticada do servidor (resposta assinada / nonce por
     sessão).
   - Adicione "tempo de revogação" de licença/usuário no servidor — o cliente não pode mais decidir sozinho.
2. **Se o produto precisar funcionar 100% offline:** documente explicitamente o login como **proteção soft**
   (anti-curiosidade) e priorize o item seguinte (chaves), que é o que realmente protege os dados.
3. Nunca confie em `lockout`/contadores locais como controle de segurança.

### P0 — Segredo criptográfico (resolve V3)

4. **Elimine o fallback para a chave padrão embutida** (`GetActiveKeyMaterial` em `CryptoUtils.cs:198`).
5. **Derive a chave de arquivo de um segredo do usuário**, não de uma constante do binário:
   - Ex.: `AES key = HKDF( senha/DPAPI ), por arquivo `salt` aleatório (16 bytes) — já existe a infra de salt/HKDF.
6. **Use DPAPI (Windows)** para guardar a chave mestre por usuário/instalação:
   ```csharp
   // Exemplo — guardar chave mestre só do usuário Windows
   var encrypted = ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser);
   File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(
       Environment.SpecialFolder.LocalApplicationData), "cryptotxt.key"), encrypted);
   ```
   - DPAPI amarra a chave ao perfil Windows (ataque exige acesso local ao mesmo usuário).
   - Alternativa mais forte: TPM (Windows Hello / CNG `NCryptProtectSecret`).
7. **Criptografe o arquivo de chave exportado ("CSK3")** com uma passphrase (KDF), em vez de apenas
   `magic + key + IV` em claro (`SharedCrypto.CreateProtectedKeyFileBytes`).

### P1 — Integridade e distribuição (resolve V4, e dificulta V2)

8. **Assine o executável** (Authenticode EV) e **verifique a assinatura no próprio código** antes do `Main`,
   comparando com o hash esperado (pin).
   - Lembrete: verificação local pode ser removida por quem tem o binário; é *detecção/deterrência*, não
     defesa absoluta.
9. **Adicione checagem de integridade do bundle:**
   - Guarde hash (SHA-256) de `CryptoTxt.dll` ofuscado + "semente" derivada de atributos do assembly;
   - Compare em runtime em pelo menos 2 pontos (início e em background).
10. **Ofuscação** (darumente depois das correções de modelo): ConfuserEx / Agile.NET / obfuscação comercial para
    nome, fluxo de controle e strings — eleva o custo da engenharia reversa, mas **não** é substrato da
    autenticação. (Nota: ofuscadores comerciais "de segurança" tipo VMProtect/Themida atrapalham análise
    dinâmica, mas continuation: atacante com tempo sempre ganha.)
11. **Anti-debug com profundidade (só como camada):** checar `NtQueryInformationProcess(ProcessDebugPort)`,
    hardware breakpoints, window title/classes, timing de execução; rodar a checagem em thread dedicada. Novamente:
    camada de atrito, não defesa.

### P2 — Higiene (resolve V7 e melhoria geral)

12. **Não exibir dica de senha** (ou torná-la opcional/off por padrão). Reduz o espaço de busca.
13. **Logue/telementrie tentativas e integridade** (se houver servidor) para detectar distribuição adulterada.
14. **Revise o fluxo "Gerar Chave/Exportar Chave"**: garantir que o usuário saiba quando está usando a chave
    padrão (UI: aviso "chave padrão embutida — não é um segredo") e incentivar geração de chave própria.

---

## 8. O que NÃO foi alterado

O patch aplicado em `CryptoTxt-nopass.exe` **apenas** neutralizou a checagem de login (`ValidateCredentials`).
O algoritmo de criptografia de arquivos (AES-GCM "CSG3", HKDF, layout do `.txt.enc`) permaneceu idêntico e
continuou funcionando — o que reforça que a integridade/lógica de dados não foi afetada, mas também que a **chave
padrão** segue recuperável e utilizável para descriptografar arquivos atuais.

---

## 9. Anexos — locais de código citados

| Arquivo (decompilado) | Linhas | Assunto |
|---|---|---|
| `CryptoTxt/Program.cs` | 12–33 | Anti-debug (`IsDebuggerPresent`) + thread de policiamento |
| `CryptoTxt/LoginForm.cs` | 103–120 | Decisão de login na UI |
| `CryptoTxt.Utils/LoginConfiguration.cs` | 22–41 | `ValidateCredentials` (PBKDF2 + FixedTimeEquals) — patch aplicado |
| `CryptoTxt.Utils/LoginConfigurationLoader.cs` | 34–107 | Parse do `login.txt` (rejeita credencial em claro) |
| `CryptoTxt.Utils/CryptoUtils.cs` | 198–205 | `GetActiveKeyMaterial` (fallback chave padrão) |
| `CryptoCommon/SharedCrypto.cs` | 16–42 | `EmbeddedDefaultKey`/`EmbeddedDefaultIV` + formatação CSG3/CSK3 |
| `CryptoTxt.login.txt` | 1–6 | Usuário, salt, hash, iterações, dica `r...A` |

---

## 10. Conclusão

O bypass de senha do CryptoTxt é **trivial e determinístico** porque a autenticação e o segredo criptográfico
residem no binário distribuído ao usuário. As correções de maior impacto são:

1. **Autenticação no servidor** (ou aceitar explicitamente que o login local é "soft").
2. **Remover a chave padrão embutida**; derivar/guardar chaves com DPAPI/TPM e segredo do usuário.
3. **Assinatura + integridade + ofuscação** como camadas de atrito e detecção de adulteração.

Nenhuma medida puramente local dará proteção forte para login/chave neste modelo. A decisão mais importante a
tomar no produto é: **onde mora a confiança?** — e movê-la para fora do binário do cliente (servidor/TPM).