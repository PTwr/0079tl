descriptionTextSet = {
	{
		text_name = "Subtitles",
		link_model = nil,
		link_name = nil,
		isdisp = false,
	}
}

endingSubs = {
	--CEnding_END_EFF_1 bgm_61
	{
		--Alan Aylward narrative
		{
			fromMS = 11000 ,
			toMS = 13960,
			text = "Our battle ends here for now."
		},
		{
			fromMS = 13960,
			toMS = 19080,
			text = "With Zeon troops defeated"
		},
		{
			fromMS = 19080,
			toMS = 20680,
			text = "Africa mopup operation is completed."
		},
		{
			fromMS = 20680,
			toMS = 25880,
			text = "However, many Zeon troops are left hiding underground"
		},
		{
			fromMS = 25880,
			toMS = 29800,
			text = "and Africa will remain Zeon holdout for years to come."
		},
		{
			fromMS = 29800,
			toMS = 33100,
			text = "But that's another story."
		},
		{
			fromMS = 36100,
			toMS = 38300,
			text = "Fighting in space will soon come to an end"
		},
		{
			fromMS = 38300,
			toMS = 42900,
			text = "in this worst war in human history."
		},
		{
			fromMS = 42900,
			toMS = 46040,
			--confidence -> combat skills? manliness?
			text = "All I gained is confidence"
		},
		{
			fromMS = 46040,
			toMS = 48480,
			text = "and reliable friends."
		},
		{
			fromMS = 48480,
			toMS = 52160,
			text = "But that's enough."
		},
		{
			fromMS = 55160,
			toMS = 57420,
			text = "A new story will start from here..."
		},
		--Char Aznable narrative
		{
			fromMS = 60000 + 35000,
			toMS = 60000 + 38240,
			text = "While Zeon mopup operation was unfolding on ground"
		},
		{
			fromMS = 60000 + 38240,
			toMS = 60000 + 43640,
			text = "Main battle in space was reaching its final phase."
		},
		{
			fromMS = 60000 + 43640,
			toMS = 60000 + 49660,
			text = "After capturing space fortress Solomon"
		},
		{
			fromMS = 60000 + 49660,
			toMS = 60000 + 53580,
			text = "Federation used it as stepping stone toward A Baoa Qu"
		},
		{
			fromMS = 60000 + 53580,
			toMS = 60000 + 56420,
			text = "Zeon's final line of defense"
		},
		{
			fromMS = 60000 + 56420,
			toMS = 120000 + 01340,
			text = "And succeded in defeating the enemy \
despite suffering great losses."
		},
		--Second part of Char's narrative
		{
			fromMS = 120000 + 03340,
			toMS = 120000 + 05340,
			text = "Universal Century, 0080"
		},
		{
			fromMS = 120000 + 05340,
			toMS = 120000 + 08280,
			text = "January 1"
		},
		{
			fromMS = 120000 + 08280,
			toMS = 120000 + 10820,
			text = "In a Lunar city of Granada"
		},
		{
			fromMS = 120000 + 10820,
			toMS = 120000 + 16060,
			text = "A peace treaty was signed between the Earth Federation\
and goverment of Republic of Zeon"
		},
		{
			fromMS = 120000 + 18060,
			toMS = 120000 + 22100,
			text = "This long and fierce war, later named One Year War, \
finally came to an end."
		},
		{
			fromMS = 120000 + 25260,
			toMS = 120000 + 26260,
			text = "However"
		},
		{
			fromMS = 120000 + 26260,
			toMS = 120000 + 29560,
			text = "There are many Zeon remnants that view this peace as unjust"
		},
		{
			fromMS = 120000 + 29560,
			toMS = 120000 + 31840,
			text = "And for many years to come"
		},
		{
			fromMS = 120000 + 31840,
			toMS = 120000 + 36020,
			text = "There will be no true peace in Earth Sphere."
		},
		{
			fromMS = 120000 + 38020,
			toMS = 120000 + 41020,
			text = "Warriors will rest for a moment"
		},
		{
			fromMS = 120000 + 41020,
			toMS = 120000 + 44320,
			text = "Praying for the lost souls"
		},
		{
			fromMS = 120000 + 44320,
			toMS = 120000 + 49320,
			text = "Until the time comes again"
		},
		{
			fromMS = 120000 + 49320,
			toMS = 120000 + 52920,
			text = "To hunt for Steel Giants."
		},
	},
	--CEnding_END_EFF_2 bgm_62
	{

	},
	--CEnding_END_ZEON_1 bgm_63
	{

	},
	--CEnding_END_ZEON_2 bgm_64
	{

	},
	--CEnding_END_ZEON_3 bgm_65
	{

	},
}

sub = nil
subs = {}
subId = 1
frameCounter = 0
function descReplacer(str)
	time = frameCounter / 60 --frame per Second
	time = time * 1000 --time is in Miliseconds
	--return ""..frameCounter
	if (sub ~= nil) then
		--wait for sub display time
		if (sub.fromMS <= time) then
			--if sub expired
			if (sub.toMS < time) then
				--fetch next sub
				subId = subId+1
				sub = subs[subId]
			else
				return sub.text
			end
		else
			return "";
		end
	end
	return "";
end

rollStart = 1
function RollWithSubs(textSet)
	
	--pick subs set
	subs = endingSubs[GetEndingType()]
	sub = subs[1] --prefetch first sub

	pos = {}

    --preposition credits
	for i = 1, #textSet, 1 do
		pos[i] = 480 + (30 * (i-1))
	end

	 -- Sleep(180) between BgmPlay and Roll()
	for frame=180,11000,1 do
		frameCounter = frame
		for i = rollStart, #textSet, 1 do
		
			--stop wasting cycles on offscreen text
			if(pos[i]<-50)then
				rollStart = rollStart + 1
				Text_SetIsDisp(pos[i], false)
			else
				--move text up a bit
				pos[i] = pos[i] - up_speed
				Text_SetPos(textSet[i].obj, 330, pos[i], -1)

				if (pos[i]<280) then
					Text_SetIsDisp(textSet[i].obj, true)
				end
			end
		end
	    		
		--some kind of wait for animation to finish?
		CheckSkip()
		if(IsSkip)then
			skip_count = skip_count - 1
			if(skip_count < 0)then
				break
			end
		end
		
		Sleep()
	end
end