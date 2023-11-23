-- 菊池用共用関数群及び宣言群 
-- 2007/01/19
-- 菊池


if (__tk_utility__ == nil) then -- #pragra once 代わり 
    __tk_utility__ = true


--π
PI = 3.14159265358979323846


--ＳＥ定義
SE_CURSOR		= SE.Cursor		--カーソル移動音 
SE_OK       	= SE.OK			--決定音 
SE_CANCEL   	= SE.Cancel		--キャンセル音
SE_WARNING 		= SE.Warning01	--警告音 



-- フェード時間
FADE_SHORT_F	= 30.0
FADE_SHORT_I	= 30
FADE_MIDDLE_F   = 60.0
FADE_MIDDLE_I   = 60
FADE_LONG_F		= 120.0
FADE_LONG_I		= 120 



--汎用ウィンドウカーソル位置
CURSOR_YES		= 1
CURSOR_NO		= 2 


--陣営
EFF		= 1
ZEON	= 2


LastReqAnim = nil

-- アニメーションセットを再生し、再生しているアニメーションセットを保存
function SetAnimSetSave(animSet)

	if(animSet == nil)then
		Dbg_Print("アニメーションセットの指定が不正です\n")
		return
	end
	
	LastReqAnim = animSet
	
	SetAnimSet(LastReqAnim)

end

function GetLastAnimSetFinish()

	local finish = GetAnimSetFinish(LastReqAnim)
	
	if(finish)then
		LastReqAnim = nil
	end

	return finish
end

-- アニメーションセットを再生した後、終了までその場で待つ 
function WaitAnimSetFinish(animSet)

	if(animSet == nil)then
		Dbg_Print("アニメーションセットの指定が不正です\n")
		return
	end

	while(GetAnimSetFinish(animSet) == false)do
		Sleep()
	end

end

-- アニメーションセットのリストを順次再生 
function SetAnimSetList(animSetList)

	for a = 1, #animSetList, 1 do
	    SetAnimSetSave(animSetList[a])
	    if(a ~= #animSetList)then -- ラストでないなら
			WaitAnimSetFinish(animSetList[a])-- 終了を待つ
		end
	end

end

-- アニメーションセットのリストを全部再生 
function SetAnimSetListNoWait(animSetList)

	--Dbg_Print("#animSetList   = "..#animSetList)
	--Dbg_Print("#animSetList[1]   = "..#animSetList[1])

	for a = 1, #animSetList, 1 do
	    SetAnimSetSave(animSetList[a])
	end

end

-- 名指しでテキストの色変え
function Text_SetColorName(textSet, name, R, G, B)

	if(textSet == nil)then return end

	index = GetTextIndex(textSet, name)
	
	if(index ~= -1)then
		Text_SetColor(textSet[index].obj, R, G, B)
	end

end 

end -- if (__tk_utility__ == nil)

