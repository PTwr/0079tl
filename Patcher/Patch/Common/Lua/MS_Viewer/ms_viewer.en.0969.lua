--hide description if voice stopped for whatever reason
if(Snd_VoiceGetPlay()==false)then
    DescBlank()
end