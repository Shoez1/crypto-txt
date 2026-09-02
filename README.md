# CryptoTxt

CryptoTxt é um utilitário Windows Forms exclusivo para criptografar e descriptografar arquivos de texto `.txt`.

## Segurança

- Login embutido no EXE com senha em hash PBKDF2-SHA256, salt aleatório e comparação em tempo constante.
- Bloqueio temporário após 5 tentativas de login inválidas.
- Arquivos `.txt.enc` usam o padrão único `CSG3`: AES-GCM autenticado, salt e nonce aleatórios por arquivo.
- A chave padrão é embutida no executável e é a mesma em CryptoFotos, CryptoMulti e CryptoTxt.
- Exportação/importação de chave usa somente o padrão `CSK3`, sem senha no arquivo de chave.
- A interface e as validações aceitam apenas `.txt` para criptografar e `.txt.enc` para descriptografar/visualizar.
- **Anti-debug profundo** no EXE de Release: `Debugger.IsAttached`, `IsDebuggerPresent`, `CheckRemoteDebuggerPresent`, `NtQueryInformationProcess` (debug port/object/flags), flags `BeingDebugged`/`NtGlobalFlag` do PEB e hardware breakpoints (DR0–DR3), além de verificação de janelas/processos de depuradores. Rodado no startup e em thread watchdog com intervalos aleatórios.
- **Anti-tamper**: o EXE publicado carrega no final uma assinatura RSA-2048 (overlay `CTXSIGN1`) gerada **a cada build**. Em runtime o `IntegrityGuard` verifica a assinatura do próprio executável contra a chave pública embutida, e o watchdog confere periodicamente. O mesmo bypass por patch + reempacotamento do bundle passa a ser detectado (o exe encerra).
- Modo **auto-pin** (fallback): builds não assinados (ex.: `dotnet build` de dev) gravam a hash SHA-256 do EXE em `%LOCALAPPDATA%\CryptoTxt\integrity.dat` no primeiro uso e rejeitam divergências depois.
- Nota: estas camadas são **deterrência/atrito**, não proteção absoluta — um atacante com tempo e acesso ao binário sempre pode removê-las. A confiança real dos dados continua nas chaves de criptografia (veja "Chaves").

## Build Portátil

Execute na raiz do projeto:

```bat
build-exe.bat
```

O script:

1. Gera ou atualiza `login.txt` com hash seguro.
2. Executa `dotnet restore`.
3. Gera um par de chaves RSA-2048 **novo a cada build**, escreve `Security/IntegrityToken.g.cs` com a chave pública e executa `dotnet publish` (single-file self-contained).
4. Assina o EXE produzido (SHA-256 + RSA) e acrescenta o overlay `CTXSIGN1` no final do arquivo.
5. Restaura `Security/IntegrityToken.g.cs` para o stub de desenvolvimento (nenhum segredo fica no repositório).
6. Salva o log em `build-exe.log`.

> Builds feitos com `dotnet build`/`dotnet publish` diretos **não** assinam o EXE (o token fica vazio) e o app usa o modo auto-pin. Use `build-exe.bat` para distribuir EXE com assinatura de integridade.

Para rodar sem pausa final:

```bat
build-exe.bat --no-pause
```

## Login

Não edite credenciais em texto claro. Use:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Generate-Login.ps1
```

O formato gerado é:

```txt
version:1.4
usuario:seu_usuario
salt:...
senhahash:...
iteracoes:310000
dicadesenha:sua dica opcional
```

As opções antigas `debug:` e `senhapadrao:` foram removidas por segurança.

## Uso

1. Abra `CryptoTxt.exe`.
2. Faça login com o usuário e senha definidos no build.
3. Selecione um arquivo `.txt` ou uma pasta com arquivos `.txt`.
4. Clique em `Criptografar` para gerar `.txt.enc`.
5. Selecione um `.txt.enc` ou uma pasta com `.txt.enc`.
6. Clique em `Descriptografar` para restaurar os arquivos.

## Chaves

- `Exportar Chave` salva a chave ativa como `CSK3`, sem senha.
- `Importar Chave` aceita somente chave `CSK3` e ativa essa chave até ser desativada.
- `Gerar Chave` cria uma chave totalmente nova, carrega essa chave na sessão atual e permite exportá-la em seguida.
- A mesma chave funciona em CryptoFotos, CryptoMulti e CryptoTxt.
- Guarde chaves exportadas fora do repositório e não compartilhe junto com arquivos criptografados.

## Testes Automatizados

Para executar a suíte de testes unitários:

```bash
dotnet test tests/CryptoTxt.Tests/CryptoTxt.Tests.csproj
```

