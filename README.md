# CryptoTxt

CryptoTxt é um utilitário moderno para Windows que permite criptografar e descriptografar arquivos `.txt` de forma simples, segura e portátil.

## Funcionalidades
- **Criptografia forte (AES-256)**: Apenas o programa pode descriptografar os arquivos.
- **Proteção por login**: O acesso ao programa é restrito por usuário e senha, definidos antes do build.
- **Interface intuitiva**: Basta selecionar o arquivo e clicar em "Criptografar" ou "Descriptografar".
- **Executável portátil**: Após o build, o programa roda como um único `.exe` sem necessidade de arquivos externos.
- **Ícone personalizado**: O executável possui ícone próprio (crypt.ico).

## Como usar

### 1. Defina o login e senha
Edite o arquivo `login.txt` na raiz do projeto antes de compilar. Exemplo:
```
admin:123456
```

### 2. (Opcional) Troque a chave de criptografia
Para maior segurança, você pode alterar a chave e o IV (vetor de inicialização) usados na criptografia:
- Abra o arquivo `Utils/CryptoUtils.cs`.
- Substitua os valores dos arrays `key` (32 bytes) e `iv` (16 bytes) por outros valores aleatórios.
- Exemplo de como gerar novos valores:
  - Use um gerador de bytes aleatórios seguro, como o site [random.org](https://www.random.org/bytes/) ou um script em Python/C#.
- **Atenção:** Só arquivos criptografados com a mesma chave/IV poderão ser descriptografados pelo programa.

### 3. Compile o executável portátil
Execute:
```sh
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true
```
O executável estará em `bin/Release/net6.0-windows/win-x64/publish/CryptoTxt.exe`.

### 3. Rode o programa
- Dê dois cliques no `CryptoTxt.exe`.
- Faça login com o usuário e senha definidos.
- Selecione um arquivo `.txt` ou `.enc`.
- Clique em "Criptografar" para gerar um `.enc`.
- Clique em "Descriptografar" para gerar um `.decrypted.txt`.

### 4. Portabilidade
Você pode copiar apenas o `CryptoTxt.exe` para qualquer computador com Windows 10/11 e rodar normalmente.

## Observações Técnicas
- O login é embutido no executável como recurso, não ficando exposto após o build.
- A chave de criptografia é fixa e interna ao código.
- O programa mostra mensagens de erro claras caso haja problemas com o login ou arquivos.
- O ícone do executável é definido por `crypt.ico` (adicione ou substitua antes do build).

## Segurança
- Apenas quem possui o programa consegue descriptografar os arquivos.
- Recomenda-se não compartilhar o `.exe` se quiser manter os arquivos protegidos.

## Licença
MIT

---

Desenvolvido com ❤️ para uso pessoal e educacional. Se encontrar bugs ou quiser contribuir, abra uma issue ou pull request!
