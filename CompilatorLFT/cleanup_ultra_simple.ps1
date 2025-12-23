# ╔═══════════════════════════════════════════════════════════════╗
# ║           COMPILATOR LFT - CLEANUP ULTRA SIMPLE               ║
# ║                   Script pentru Windows                        ║
# ╚═══════════════════════════════════════════════════════════════╝

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           COMPILATOR LFT - CLEANUP ULTRA SIMPLE               ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Directorul curent
$DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $DIR

# ═══════════════════════════════════════════════════════════════
# PASUL 1: BACKUP
# ═══════════════════════════════════════════════════════════════

$BACKUP_DIR = "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "[1/4] Creez backup in $BACKUP_DIR..." -ForegroundColor Yellow

New-Item -ItemType Directory -Path $BACKUP_DIR -Force | Out-Null

if (Test-Path "Program.cs") {
    Copy-Item "Program.cs" "$BACKUP_DIR\" -Force
}
if (Test-Path "Core") {
    Copy-Item "Core" "$BACKUP_DIR\" -Recurse -Force
}

Write-Host "      ✓ Backup creat!" -ForegroundColor Green
Write-Host ""

# ═══════════════════════════════════════════════════════════════
# PASUL 2: STERGERE FISIERE NEFOLOSITOARE
# ═══════════════════════════════════════════════════════════════

Write-Host "[2/4] Sterg fisierele nefolositoare..." -ForegroundColor Yellow

# Stergere VM folder
if (Test-Path "Core\VM") {
    Remove-Item "Core\VM" -Recurse -Force
    Write-Host "      ✓ Sters: Core\VM\" -ForegroundColor Green
}

# Stergere REPL.cs
if (Test-Path "Core\REPL.cs") {
    Remove-Item "Core\REPL.cs" -Force
    Write-Host "      ✓ Sters: Core\REPL.cs" -ForegroundColor Green
}

# Stergere ThreeAddressCodeGenerator.cs
if (Test-Path "Core\ThreeAddressCodeGenerator.cs") {
    Remove-Item "Core\ThreeAddressCodeGenerator.cs" -Force
    Write-Host "      ✓ Sters: Core\ThreeAddressCodeGenerator.cs" -ForegroundColor Green
}

# Stergere PostfixGenerator.cs
if (Test-Path "Core\PostfixGenerator.cs") {
    Remove-Item "Core\PostfixGenerator.cs" -Force
    Write-Host "      ✓ Sters: Core\PostfixGenerator.cs" -ForegroundColor Green
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════
# PASUL 3: INSTALARE PROGRAM NOU
# ═══════════════════════════════════════════════════════════════

Write-Host "[3/4] Instalez noul Program.cs..." -ForegroundColor Yellow

if (Test-Path "Program_ULTRA_SIMPLE.cs") {
    Copy-Item "Program_ULTRA_SIMPLE.cs" "Program.cs" -Force
    Write-Host "      ✓ Program.cs actualizat!" -ForegroundColor Green
} else {
    Write-Host "      ✗ EROARE: Program_ULTRA_SIMPLE.cs nu exista!" -ForegroundColor Red
    Write-Host "      Asigura-te ca ai copiat fisierul in acest folder." -ForegroundColor Red
    exit 1
}

Write-Host ""

# ═══════════════════════════════════════════════════════════════
# PASUL 4: VERIFICARE
# ═══════════════════════════════════════════════════════════════

Write-Host "[4/4] Verific structura..." -ForegroundColor Yellow
Write-Host ""

Write-Host "  Fisiere Core\ ramase:" -ForegroundColor White
if (Test-Path "Core") {
    Get-ChildItem "Core\*.cs" | ForEach-Object {
        Write-Host "    ├── $($_.Name)" -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                    CLEANUP COMPLET!                           ║" -ForegroundColor Green
Write-Host "╠═══════════════════════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "║                                                               ║" -ForegroundColor Green
Write-Host "║  Urmatorul pas:                                               ║" -ForegroundColor Green
Write-Host "║    > dotnet build                                             ║" -ForegroundColor Green
Write-Host "║    > dotnet run                                               ║" -ForegroundColor Green
Write-Host "║                                                               ║" -ForegroundColor Green
Write-Host "║  Pentru restaurare backup:                                    ║" -ForegroundColor Green
Write-Host "║    > Copy-Item $BACKUP_DIR\Program.cs ." -ForegroundColor Green
Write-Host "║    > Copy-Item $BACKUP_DIR\Core\* Core\ -Recurse" -ForegroundColor Green
Write-Host "║                                                               ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
