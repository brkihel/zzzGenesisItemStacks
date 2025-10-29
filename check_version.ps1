# Script para verificar versão do assembly
$dllPath = "zzzGenesisItemStacks\bin\Release\net48\zzzGenesisItemStacks.dll"

if (Test-Path $dllPath) {
    Write-Host "=== Verificação de Versão do Assembly ===" -ForegroundColor Green
    Write-Host ""
    
  # Carregar assembly
    $assembly = [System.Reflection.Assembly]::LoadFile((Resolve-Path $dllPath).Path)
    
  # Versão do Assembly
    $version = $assembly.GetName().Version
    Write-Host "AssemblyVersion: $version" -ForegroundColor Cyan
 
    # File Version
    $fileInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($assembly.Location)
    Write-Host "FileVersion: $($fileInfo.FileVersion)" -ForegroundColor Cyan
    Write-Host "ProductVersion: $($fileInfo.ProductVersion)" -ForegroundColor Cyan
    
    # Custom attributes
Write-Host ""
    Write-Host "=== Assembly Attributes ===" -ForegroundColor Green
    $attrs = $assembly.GetCustomAttributesData()
    foreach ($attr in $attrs) {
        if ($attr.AttributeType.Name -like "*Version*" -or $attr.AttributeType.Name -like "*Assembly*") {
        Write-Host "$($attr.AttributeType.Name): $($attr.ConstructorArguments[0].Value)" -ForegroundColor Yellow
  }
    }
    
    Write-Host ""
    Write-Host "=== PluginInfo Class ===" -ForegroundColor Green
    $pluginInfoType = $assembly.GetType("GenesisItemStacks.PluginInfo")
    if ($pluginInfoType) {
        $modVersion = $pluginInfoType.GetField("ModVersion").GetValue($null)
        $modVersionFull = $pluginInfoType.GetField("ModVersionFull").GetValue($null)
   $modGUID = $pluginInfoType.GetField("ModGUID").GetValue($null)
        
        Write-Host "ModVersion: $modVersion" -ForegroundColor Cyan
        Write-Host "ModVersionFull: $modVersionFull" -ForegroundColor Cyan
        Write-Host "ModGUID: $modGUID" -ForegroundColor Cyan
    }
    
} else {
 Write-Host "DLL not found at: $dllPath" -ForegroundColor Red
}
