# CryptoTxt

CryptoTxt é um utilitário Windows Forms exclusivo para criptografar e descriptografar arquivos de texto `.txt`.

## Segurança

- Login embutido no EXE com senha em hash PBKDF2-SHA256, salt aleatório e comparação em tempo constante.
- Bloqueio temporário após 5 tentativas de login inválidas.
- Arquivos `.txt.enc` usam o padrão único `CSG3`: AES-GCM autenticado, salt e nonce aleatórios por arquivo.
- A chave padrão é embutida no executável e é a mesma em CryptoFotos, CryptoMulti e CryptoTxt.
- Exportação/importação de chave usa somente o padrão `CSK3`, sem senha no arquivo de chave.
- A interface e as validações aceitam apenas `.txt` para criptografar e `.txt.enc` para descriptografar/visualizar.

## Build Portátil

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
4. Clique em `Criptografar` para gerar `.txt.enc`.
5. Selecione um `.txt.enc` ou uma pasta com `.txt.enc`.
6. Clique em `Descriptografar` para restaurar os arquivos.

## Chaves

- `Exportar Chave` salva a chave ativa como `CSK3`, sem senha.
- `Importar Chave` aceita somente chave `CSK3` e ativa essa chave até ser desativada.
- `Gerar Chave` cria uma chave totalmente nova, carrega essa chave na sessão atual e permite exportá-la em seguida.
- A mesma chave funciona em CryptoFotos, CryptoMulti e CryptoTxt.
- Guarde chaves exportadas fora do repositório e não compartilhe junto com arquivos criptografados.
