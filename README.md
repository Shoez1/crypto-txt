# CryptoTxt

CryptoTxt é um utilitário Windows Forms para criptografar e descriptografar arquivos `.txt` em um executável portátil single-file.

## Segurança

- Login embutido no EXE com senha em hash PBKDF2-SHA256, salt aleatório e comparação em tempo constante.
- Bloqueio temporário após 5 tentativas de login inválidas.
- Novos arquivos `.enc` usam AES-GCM com salt e nonce aleatórios por arquivo.
- Arquivos antigos do CryptoTxt em AES-CBC ainda podem ser descriptografados por compatibilidade.
- A chave local padrão é criada por usuário em `%LOCALAPPDATA%\CryptoTxt\user-key.dat` e protegida com DPAPI.
- Para abrir arquivos em outro Windows/usuário, exporte a chave e importe no outro ambiente.

## Build portátil

Execute na raiz do projeto:

```bat
build-exe.bat
```

O script:

1. Gera ou atualiza `login.txt` com hash seguro.
2. Executa `dotnet restore`.
3. Publica um EXE single-file self-contained em `bin\Release\net10.0-windows\win-x64\publish\CryptoTxt.exe`.
4. Salva o log em `build-exe.log`.

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
4. Clique em `Criptografar` para gerar `.enc`.
5. Selecione um `.enc` ou uma pasta com `.enc`.
6. Clique em `Descriptografar` para restaurar os arquivos.

## Chaves

- `Exportar Chave` salva a chave ativa em Base64 para backup ou migração.
- `Importar Chave` ativa uma chave exportada até ser desativada.
- Guarde chaves exportadas fora do repositório e não compartilhe junto com arquivos criptografados.
