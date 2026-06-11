$path = 'd:/ACA/SILWeb/CuposCorretajeWeb/Views/Solicitudes/AltaSolicitud.cshtml'
$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)

# Construir los valores de reemplazo a partir de sus code points Unicode
# para que el archivo de script no dependa del encoding de la consola.
function U($cp) { return [char]::ConvertFromUtf32($cp) }

$map = [ordered]@{
    (U 0x00E1) = 'a'
    (U 0x00E9) = 'e'
    (U 0x00ED) = 'i'
    (U 0x00F3) = 'o'
    (U 0x00FA) = 'u'
    (U 0x00C1) = 'A'
    (U 0x00C9) = 'E'
    (U 0x00CD) = 'I'
    (U 0x00D3) = 'O'
    (U 0x00DA) = 'U'
    (U 0x00F1) = 'n'
    (U 0x00D1) = 'N'
    (U 0x00BF) = '?'
    (U 0x00A1) = '!'
    (U 0x2014) = '-'
    (U 0x00B7) = '*'
}

# Armar el escape JS concatenando partes: backslash + u + 00 + hex
$total = 0
foreach ($k in $map.Keys) {
    $cp = [int][char]$k
    $hex = $cp.ToString('x4')
    $esc = '\' + 'u' + $hex.Substring(0,2) + $hex.Substring(2,2)
    $count = ($content.Length - $content.Replace($k, '').Length) / $k.Length
    if ($count -gt 0) {
        $content = $content.Replace($k, $esc)
        $total += [int]$count
    }
}

[System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
Write-Output ('Total reemplazos: {0}' -f $total)
