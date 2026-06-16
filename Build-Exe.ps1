param(
    [string]$Project = "CryptoTxt.csproj",
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$ForceLogin,
    [string]$UserName,
    [string]$PasswordPlaintext,
    [string]$Hint
)

$ErrorActionPreference = "Stop"
$hintParameterProvided = $PSBoundParameters.ContainsKey("Hint")

$projectRoot = $PSScriptRoot
$projectPath = Join-Path $projectRoot $Project
$loginPath = Join-Path $projectRoot "login.txt"
$generateLoginScript = Join-Path $projectRoot "Generate-Login.ps1"
$logFile = Join-Path $projectRoot "build-exe.log"

[System.IO.File]::WriteAllText($logFile, "")

function Write-Log {
    param(
        [string]$Level,
        [string]$Message
    )

    $line = "[{0}] {1}" -f $Level, $Message
    Write-Host $line
    Add-Content -Path $logFile -Value $line
}

function Read-RequiredValue {
    param(
        [string]$Prompt,
        [string]$CurrentValue
    )

    if (-not [string]::IsNullOrWhiteSpace($CurrentValue)) {
        return $CurrentValue
    }

    $value = Read-Host $Prompt
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw ("{0} invalido." -f $Prompt)
    }

    return $value
}

function Read-PasswordValue {
    param([string]$CurrentValue)

    if (-not [string]::IsNullOrWhiteSpace($CurrentValue)) {
        return $CurrentValue
    }

    $securePassword = Read-Host "Senha do programa" -AsSecureString
    $password = [System.Net.NetworkCredential]::new('', $securePassword).Password
    if ([string]::IsNullOrWhiteSpace($password)) {
        throw "Senha invalida."
    }

    return $password
}

function Invoke-And-Log {
    param(
        [scriptblock]$Action
    )

    & $Action 2>&1 | ForEach-Object {
        $text = $_.ToString()
        Write-Host $text
        Add-Content -Path $logFile -Value $text
    }
}

function Test-LoginFileHasSecureHash {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return $false
    }

    $lines = Get-Content -LiteralPath $Path
    $hasUser = $false
    $hasSalt = $false
    $hasHash = $false
    $hasIterations = $false

    foreach ($line in $lines) {
        if ($line -match '^\s*(debug|senhapadrao)\s*:') {
            return $false
        }
        if ($line -match '^\s*(usuario|user)\s*:') {
            $hasUser = $true
        }
        elseif ($line -match '^\s*salt\s*:') {
            $hasSalt = $true
        }
        elseif ($line -match '^\s*(senhahash|passwordhash)\s*:') {
            $hasHash = $true
        }
        elseif ($line -match '^\s*(iteracoes|iterations)\s*:') {
            $hasIterations = $true
        }
    }

    return $hasUser -and $hasSalt -and $hasHash -and $hasIterations
}

function Test-LoginFileHasHint {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return $false
    }

    return [bool](Select-String -LiteralPath $Path -Pattern '^\s*dicadesenha\s*:\s*\S+' -Quiet)
}

function Set-LoginFileHint {
    param(
        [string]$Path,
        [string]$HintValue
    )

    if ([string]::IsNullOrWhiteSpace($HintValue)) {
        return $false
    }

    $encoding = New-Object System.Text.UTF8Encoding($false)
    $newLines = New-Object System.Collections.Generic.List[string]
    $hintWritten = $false

    foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
        if ($line -match '^\s*dicadesenha\s*:') {
            if (-not $hintWritten) {
                $newLines.Add(("dicadesenha:{0}" -f $HintValue))
                $hintWritten = $true
            }
        }
        else {
            $newLines.Add($line)
        }
    }

    if (-not $hintWritten) {
        $newLines.Add(("dicadesenha:{0}" -f $HintValue))
    }

    [System.IO.File]::WriteAllLines($Path, $newLines, $encoding)
    return $true
}

function Get-ProjectProperty {
    param(
        [xml]$ProjectXml,
        [string]$Name
    )

    $values = @($ProjectXml.Project.PropertyGroup | ForEach-Object { $_.$Name } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    if ($values.Count -eq 0) {
        return $null
    }

    return [string]$values[0]
}

try {
    Write-Log "INFO" ("Iniciando build em {0}" -f $projectRoot)

    if ($Configuration -ne "Release") {
        throw "Build de distribuicao deve usar Configuration=Release."
    }

    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "O .NET SDK nao foi encontrado no PATH. Instale o .NET SDK usado pelo projeto."
    }

    if (-not (Test-Path -LiteralPath $projectPath)) {
        throw ("Projeto '{0}' nao encontrado." -f $Project)
    }

    [xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
    $targetFramework = Get-ProjectProperty -ProjectXml $projectXml -Name "TargetFramework"
    if ([string]::IsNullOrWhiteSpace($targetFramework)) {
        throw "TargetFramework nao encontrado no projeto."
    }

    $assemblyName = Get-ProjectProperty -ProjectXml $projectXml -Name "AssemblyName"
    if ([string]::IsNullOrWhiteSpace($assemblyName)) {
        $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    }

    $publishDir = Join-Path $projectRoot ("bin\{0}\{1}\{2}\publish" -f $Configuration, $targetFramework, $RuntimeIdentifier)
    $outputExe = Join-Path $publishDir ("{0}.exe" -f $assemblyName)

    $loginHasSecureHash = Test-LoginFileHasSecureHash -Path $loginPath
    $loginHasHint = Test-LoginFileHasHint -Path $loginPath
    $generateLogin = $ForceLogin -or -not $loginHasSecureHash

    if ($generateLogin) {
        if (-not (Test-Path -LiteralPath $generateLoginScript)) {
            throw "Generate-Login.ps1 nao foi encontrado."
        }

        if (Test-Path -LiteralPath $loginPath) {
            Write-Log "AVISO" "login.txt atual nao possui credenciais seguras completas ou contem opcoes legadas; ele sera regenerado."
        }

        $userNameValue = Read-RequiredValue -Prompt "Usuario do programa" -CurrentValue $UserName
        $passwordPlaintextValue = Read-PasswordValue -CurrentValue $PasswordPlaintext

        if (-not $hintParameterProvided) {
            $Hint = Read-Host "Dica de senha (opcional)"
            $hintParameterProvided = $true
        }

        Write-Log "INFO" "Gerando login seguro com PBKDF2-SHA256."
        & $generateLoginScript -OutputPath $loginPath -UserName $userNameValue -PasswordPlaintext $passwordPlaintextValue -Hint $Hint
        if (-not $?) {
            throw "Falha ao gerar login.txt."
        }

        $passwordPlaintextValue = $null
        Write-Log "INFO" ("login.txt gerado em {0}" -f $loginPath)
    }
    else {
        if ($loginHasHint) {
            Write-Log "INFO" "login.txt seguro encontrado com dica de senha."
        }
        else {
            Write-Log "AVISO" "login.txt seguro encontrado, mas sem dica de senha."
        }

        $overwriteLogin = Read-Host "login.txt seguro encontrado. Deseja gerar um novo? [S/N]"
        if ($overwriteLogin -match '^(s|sim)$') {
            $userNameValue = Read-RequiredValue -Prompt "Usuario do programa" -CurrentValue $UserName
            $passwordPlaintextValue = Read-PasswordValue -CurrentValue $PasswordPlaintext
            if (-not $hintParameterProvided) {
                $Hint = Read-Host "Dica de senha (opcional)"
                $hintParameterProvided = $true
            }

            Write-Log "INFO" "Gerando novo login seguro com PBKDF2-SHA256."
            & $generateLoginScript -OutputPath $loginPath -UserName $userNameValue -PasswordPlaintext $passwordPlaintextValue -Hint $Hint
            if (-not $?) {
                throw "Falha ao gerar login.txt."
            }

            $passwordPlaintextValue = $null
        }
        else {
            if (-not $loginHasHint) {
                if (-not $hintParameterProvided) {
                    $Hint = Read-Host "Dica de senha (opcional)"
                    $hintParameterProvided = $true
                }

                if (Set-LoginFileHint -Path $loginPath -HintValue $Hint) {
                    Write-Log "INFO" "Dica de senha adicionada ao login.txt existente."
                }
                else {
                    Write-Log "AVISO" "Nenhuma dica de senha foi informada."
                }
            }

            Write-Log "INFO" "Usando login.txt existente."
        }
    }

    Write-Log "INFO" "Executando dotnet restore..."
    Invoke-And-Log { & dotnet restore $projectPath }
    if ($LASTEXITCODE -ne 0) {
        throw ("Falha no dotnet restore. Veja o log em '{0}'." -f $logFile)
    }

    Write-Log "INFO" "Executando dotnet publish..."
    Invoke-And-Log {
        & dotnet publish $projectPath `
            -c $Configuration `
            -r $RuntimeIdentifier `
            --self-contained true `
            /p:PublishSingleFile=true `
            /p:EnableCompressionInSingleFile=true `
            /p:DebugType=None `
            /p:DebugSymbols=false
    }
    if ($LASTEXITCODE -ne 0) {
        throw ("Falha no dotnet publish. Veja o log em '{0}'." -f $logFile)
    }

    if (Test-Path -LiteralPath $outputExe) {
        Write-Log "INFO" "Build concluido com sucesso."
        Write-Host ""
        Write-Host "EXE gerado em:"
        Write-Host $outputExe
    }
    else {
        Write-Log "AVISO" "O build terminou, mas o EXE nao foi localizado no caminho esperado."
        Write-Host $outputExe
    }

    exit 0
}
catch {
    Write-Log "ERRO" $_.Exception.Message
    exit 1
}
