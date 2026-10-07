$viewsPath = "c:\Users\XXXXXX\Downloads\citas-postas-medicas-app-main\citas-postas-medicas-app-main\WebConsultasMedicas\Views"
$files = Get-ChildItem -Path $viewsPath -Filter *.cshtml -Recurse

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw -Encoding UTF8
    
    $content = $content.Replace("Ã¡", "á")
    $content = $content.Replace("Ã©", "é")
    $content = $content.Replace("Ã³", "ó")
    $content = $content.Replace("Ãº", "ú")
    $content = $content.Replace("Ã±", "ñ")
    $content = $content.Replace("Ã‰", "É")
    $content = $content.Replace("Ã“", "Ó")
    $content = $content.Replace("Ãš", "Ú")
    $content = $content.Replace("Ã‘", "Ñ")
    $content = $content.Replace("Â¿", "¿")
    $content = $content.Replace("Â¡", "¡")
    
    # The 'i' with accent is tricky (Ã followed by soft hyphen or similar). 
    # Let's also do a regex replace for the known bytes if needed, but Replace should work for literal characters.
    $content = $content.Replace("Ã­", "í") 

    Set-Content -Path $f.FullName -Value $content -Encoding UTF8
}
