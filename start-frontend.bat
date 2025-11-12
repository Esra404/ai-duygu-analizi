@echo off
echo Frontend baslatiliyor...
cd frontend
if not exist node_modules (
    echo Node modules yukleniyor...
    call npm install
)
call npm run dev
pause

