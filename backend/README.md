To create a new standalone file:

Install compiler (pyinstaller):
```
pip3 install pyinstaller
```

Clear old build directories:
```
rf -rf build && rm -rf dist
```

Build:
```
pyinstaller backend.py --onefile --console
```

Result will be an executable in `dist` (should be the only file there).
