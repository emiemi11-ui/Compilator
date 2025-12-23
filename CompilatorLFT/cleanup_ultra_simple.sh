#!/bin/bash

# ╔═══════════════════════════════════════════════════════════════╗
# ║           COMPILATOR LFT - CLEANUP ULTRA SIMPLE               ║
# ║                   Script pentru Linux/Mac                      ║
# ╚═══════════════════════════════════════════════════════════════╝

set -e

echo ""
echo "╔═══════════════════════════════════════════════════════════════╗"
echo "║           COMPILATOR LFT - CLEANUP ULTRA SIMPLE               ║"
echo "╚═══════════════════════════════════════════════════════════════╝"
echo ""

# Directorul curent
DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$DIR"

# ═══════════════════════════════════════════════════════════════
# PASUL 1: BACKUP
# ═══════════════════════════════════════════════════════════════

BACKUP_DIR="backup_$(date +%Y%m%d_%H%M%S)"
echo "[1/4] Creez backup in $BACKUP_DIR..."

mkdir -p "$BACKUP_DIR"
cp Program.cs "$BACKUP_DIR/" 2>/dev/null || true
cp -r Core "$BACKUP_DIR/" 2>/dev/null || true

echo "      ✓ Backup creat!"
echo ""

# ═══════════════════════════════════════════════════════════════
# PASUL 2: STERGERE FISIERE NEFOLOSITOARE
# ═══════════════════════════════════════════════════════════════

echo "[2/4] Sterg fisierele nefolositoare..."

# Stergere VM folder
if [ -d "Core/VM" ]; then
    rm -rf Core/VM
    echo "      ✓ Sters: Core/VM/"
fi

# Stergere REPL.cs
if [ -f "Core/REPL.cs" ]; then
    rm -f Core/REPL.cs
    echo "      ✓ Sters: Core/REPL.cs"
fi

# Stergere ThreeAddressCodeGenerator.cs
if [ -f "Core/ThreeAddressCodeGenerator.cs" ]; then
    rm -f Core/ThreeAddressCodeGenerator.cs
    echo "      ✓ Sters: Core/ThreeAddressCodeGenerator.cs"
fi

# Stergere PostfixGenerator.cs
if [ -f "Core/PostfixGenerator.cs" ]; then
    rm -f Core/PostfixGenerator.cs
    echo "      ✓ Sters: Core/PostfixGenerator.cs"
fi

echo ""

# ═══════════════════════════════════════════════════════════════
# PASUL 3: INSTALARE PROGRAM NOU
# ═══════════════════════════════════════════════════════════════

echo "[3/4] Instalez noul Program.cs..."

if [ -f "Program_ULTRA_SIMPLE.cs" ]; then
    cp Program_ULTRA_SIMPLE.cs Program.cs
    echo "      ✓ Program.cs actualizat!"
else
    echo "      ✗ EROARE: Program_ULTRA_SIMPLE.cs nu exista!"
    echo "      Asigura-te ca ai copiat fisierul in acest folder."
    exit 1
fi

echo ""

# ═══════════════════════════════════════════════════════════════
# PASUL 4: VERIFICARE
# ═══════════════════════════════════════════════════════════════

echo "[4/4] Verific structura..."
echo ""

echo "  Fisiere Core/ ramase:"
if [ -d "Core" ]; then
    for f in Core/*.cs; do
        [ -f "$f" ] && echo "    ├── $(basename "$f")"
    done
fi

echo ""
echo "╔═══════════════════════════════════════════════════════════════╗"
echo "║                    CLEANUP COMPLET!                           ║"
echo "╠═══════════════════════════════════════════════════════════════╣"
echo "║                                                               ║"
echo "║  Urmatorul pas:                                               ║"
echo "║    $ dotnet build                                             ║"
echo "║    $ dotnet run                                               ║"
echo "║                                                               ║"
echo "║  Pentru restaurare backup:                                    ║"
echo "║    $ cp $BACKUP_DIR/Program.cs ."
echo "║    $ cp -r $BACKUP_DIR/Core/* Core/"
echo "║                                                               ║"
echo "╚═══════════════════════════════════════════════════════════════╝"
echo ""
