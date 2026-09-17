<#
.SYNOPSIS
  Da de alta o actualiza un usuario en usuarios.json (auth local temporal, reemplazo de acabase.com.ar).
  Correr este script DIRECTO EN EL SERVIDOR, en la carpeta App_Data del sitio de CuposCorretajeWeb.

.DESCRIPTION
  Calcula el hash de password con el MISMO algoritmo que valida el sitio (PBKDF2-HMACSHA1,
  210000 iteraciones, salt 16 bytes) y lo escribe en usuarios.json. No hace falta reiniciar el
  sitio ni recompilar nada: UsuariosLocalStore relee el archivo en cada login.

  Si el usuario ya existe, actualiza los campos que le pases (el resto queda igual). Si no
  existe, lo crea (Password y Centros son obligatorios para un usuario nuevo).

  Antes de escribir, guarda una copia de respaldo como usuarios.json.bak-<timestamp>.

.EXAMPLE
  # Alta de un usuario nuevo
  .\Administrar-UsuarioLocal.ps1 -Usuario JPEREZ -Nombre "Juan Perez" -Email jperez@acacoop.com.ar `
      -Password "Temporal123" -Centros ROS,BAS,BBL -AccesoCuposCorretaje -AccesoAuditoria

.EXAMPLE
  # Resetear solo la password de un usuario existente (deja permisos como estaban)
  .\Administrar-UsuarioLocal.ps1 -Usuario JPEREZ -Password "NuevaTemporal456"

.EXAMPLE
  # Actualizar los centros autorizados de un usuario existente
  .\Administrar-UsuarioLocal.ps1 -Usuario JPEREZ -Centros ROS,CBA
#>
param(
    [Parameter(Mandatory = $true)] [string]$Usuario,
    [string]$Nombre,
    [string]$Email,
    [string]$Password,
    [string[]]$Centros,
    [string]$CentroPorDefecto,
    [switch]$AccesoCuposCorretaje,
    [switch]$AccesoAuditoria,
    [switch]$AccesoConfiguracion,
    [switch]$AccesoConfiguracionCYO,
    [switch]$Inactivo,
    [string]$JsonPath = (Join-Path $PSScriptRoot "usuarios.json"),
    [int]$Iterations = 210000
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $JsonPath)) {
    throw "No se encontro $JsonPath"
}

$backup = "$JsonPath.bak-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item $JsonPath $backup
Write-Host "Backup guardado en $backup"

$root = Get-Content $JsonPath -Raw -Encoding UTF8 | ConvertFrom-Json

$usuarioNorm = $Usuario.Trim().ToUpperInvariant()
$existing = $root.usuarios | Where-Object { $_.usuario.ToUpperInvariant() -eq $usuarioNorm } | Select-Object -First 1

if (-not $existing -and (-not $Password -or -not $Centros)) {
    throw "Usuario '$Usuario' no existe todavia. Para darlo de alta son obligatorios -Password y -Centros."
}

if ($existing) {
    Write-Host "Usuario '$Usuario' ya existe, actualizando campos provistos..."
    $target = $existing
} else {
    Write-Host "Usuario '$Usuario' es nuevo, dando de alta..."
    $target = [pscustomobject]@{
        usuario                = $usuarioNorm
        nombre                 = ""
        email                  = ""
        activo                 = $true
        passwordSalt           = ""
        passwordHash           = ""
        passwordIterations     = $Iterations
        accesoCuposCorretaje   = $true
        centros                = @()
        centroPorDefecto       = ""
        accesoAuditoria        = $false
        accesoConfiguracion    = $false
        accesoConfiguracionCYO = $false
    }
    $root.usuarios = @($root.usuarios) + $target
}

if ($PSBoundParameters.ContainsKey('Nombre'))  { $target.nombre = $Nombre }
if ($PSBoundParameters.ContainsKey('Email'))   { $target.email = $Email }
if ($PSBoundParameters.ContainsKey('Centros')) { $target.centros = @($Centros) }
if ($PSBoundParameters.ContainsKey('CentroPorDefecto')) { $target.centroPorDefecto = $CentroPorDefecto }
if ($PSBoundParameters.ContainsKey('AccesoCuposCorretaje')) { $target.accesoCuposCorretaje = [bool]$AccesoCuposCorretaje }
if ($PSBoundParameters.ContainsKey('AccesoAuditoria'))        { $target.accesoAuditoria = [bool]$AccesoAuditoria }
if ($PSBoundParameters.ContainsKey('AccesoConfiguracion'))    { $target.accesoConfiguracion = [bool]$AccesoConfiguracion }
if ($PSBoundParameters.ContainsKey('AccesoConfiguracionCYO')) { $target.accesoConfiguracionCYO = [bool]$AccesoConfiguracionCYO }
# $target.activo ya viene en true (usuario nuevo) o con el valor existente (usuario ya cargado);
# solo lo tocamos si pidieron desactivarlo explicitamente.
if ($Inactivo.IsPresent) { $target.activo = $false }

if ($Password) {
    $salt = New-Object byte[] 16
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($salt)
    $pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($Password, $salt, $Iterations)
    $hash = $pbkdf2.GetBytes(32)

    $target.passwordSalt = [Convert]::ToBase64String($salt)
    $target.passwordHash = [Convert]::ToBase64String($hash)
    $target.passwordIterations = $Iterations
    Write-Host "Password actualizada para '$usuarioNorm'."
}

$jsonOut = $root | ConvertTo-Json -Depth 6
[System.IO.File]::WriteAllText($JsonPath, $jsonOut, [System.Text.UTF8Encoding]::new($false))

Write-Host "OK. '$usuarioNorm' guardado en $JsonPath"
if ($Password) {
    Write-Host "Password en texto plano (comunicarla y no guardarla en ningun lado): $Password"
}
