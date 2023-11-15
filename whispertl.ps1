clear

#fix annoying python whitespace output in console
chcp 65001 #TODO something else to display japanese hieroglyphs? this results in boxes 

#eve148 failed to translate randomly in the middle? double check, it might be some retarded censorship, both Google and DeepL translate some words as "fuhrer"

Get-ChildItem "C:\games\wii\0079\0079_unpacked\DATA\files\sound" -Recurse -Directory -Filter "*" |
Foreach-Object {

echo $_.FullName

$wh = "whisper"

$files = ""

$scribe = $_.FullName.Replace("sound","sound_transcribe")
$tl = $_.FullName.Replace("sound","sound_translate")

#mkdir $scribe
#mkdir $tl

$n = 0

Get-ChildItem $_.FullName -Filter *.wav | 
Foreach-Object {
#echo $n
    $files = $files + $_.FullName + " "
    $n++

    if ($n -gt 160)
    {
        $cmd = $files + " " + "--language ja --model large"
        $cmd_scribe = $cmd +  " --task transcribe"
        $cmd_tl = $cmd + " --task translate"
        cd $scribe
        & $wh $cmd_scribe.Split()

        cd $tl
        & $wh $cmd_tl.Split()
        $files = ""
        $n=0
    }
}
echo $files
    $cmd = $files + " " + "--language ja --model large"
    $cmd_scribe = $cmd +  " --task transcribe"
    $cmd_tl = $cmd + " --task translate"
  

cd $scribe
& $wh $cmd_scribe.Split()

cd $tl
& $wh $cmd_tl.Split()
}