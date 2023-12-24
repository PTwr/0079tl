MY_ARC_PATH 	= "_2d/Pause/PAUSE_A.arc"
TEXT_RES_PATH 	= "_2d/Font/IN_GAME_FONT.brfnt"

-- ＳＥ設定
SE_WINDOW_OPEN	= "WindowOpen"
SE_WINDOW_CLOSE	= "WindowClose"


-- マップ表示の位置のサイズ
Map 			= {}
Map.Left 		= -16.5
Map.Right		= 30.0
Map.Top			= 19.0
Map.Bottom		= -16.0

-- マップ拡大用マーカーの位置とサイズなどの情報
MapMaker 		= {}
MapMaker.Name		= "PAUSE_Move_00"
MapMaker.CenterX	= 6.68275
MapMaker.CenterY	= 1.625
MapMaker.Left		= -8.2
MapMaker.Right		= 8.2
MapMaker.Top		= 6.2
MapMaker.Bottom		= -6.2

-- 自分自身のマーカーのモデル名とアニメーション
OwnPoint		= {}
OwnPoint.Name		= "PAUSE_PLAYER_00"
OwnPoint.In		= "_05"
OwnPoint.Out		= "_06"
OwnPoint.IsShow 	= false

-- 友軍マーカーのモデル名、アニメーション名、テキスト生成時の情報
FriendPoints		= {}
FriendPoints.In		= "_05"
FriendPoints.Out 	= "_06"
-- FriendPoints.RotText = "_00"
-- FriendPoints.OutText = "_02"
FriendPoints.TextNode	= "Lunit"
FriendPoints.TextID	= "UNIT"
FriendPoints.ScaleX	= 1.0
FriendPoints.ScaleY	= 1.0
UseMultiModel( "PAUSE_Unit_00", FriendPoints, 10 ) -- 五個作成

-- 敵マーカーのモデル名、アニメーション名、テキスト生成時の情報
EnemyPoints		= {}
EnemyPoints.In		= "_05"
EnemyPoints.Out 	= "_06"
-- EnemyPoints.RotText 	= "_00"
-- EnemyPoints.OutText 	= "_02"
EnemyPoints.TextNode	= "LunitE"
EnemyPoints.TextID	= "ENEMY"
EnemyPoints.ScaleX	= 1.0
EnemyPoints.ScaleY	= 1.0
UseMultiModel( "PAUSE_UnitE_00", EnemyPoints, 25 ) -- 五個作成

-- 防衛目標マーカーのモデル名、アニメーション名、テキスト生成時の情報
DefensePoints			= {}
DefensePoints.In		= "_05"
DefensePoints.Out 		= "_06"
-- DefensePoints.RotText	= "_00"
-- DefensePoints.OutText 	= "_02"
DefensePoints.TextNode		= "Ltarget"
DefensePoints.TextID		= "UNIT_TARGET"
DefensePoints.ScaleX		= 1.0
DefensePoints.ScaleY		= 1.0
UseMultiModel( "PAUSE_Target_00", DefensePoints, 10 ) -- 五個作成

-- 攻撃目標マーカーのモデル名、アニメーション名、テキスト生成時の情報
AttackPoints			= {}
AttackPoints.In			= "_05"
AttackPoints.Out 		= "_06"
-- AttackPoints.RotText 	= "_00"
-- AttackPoints.OutText 	= "_02"
AttackPoints.TextNode		= "LtargetE"
AttackPoints.TextID		= "ENEMY_TARGET"
AttackPoints.ScaleX		= 1.0
AttackPoints.ScaleY		= 1.0
UseMultiModel( "PAUSE_TargetE_00", AttackPoints, 10 ) -- 五個作成

-- 供給場所マーカーのモデル名、アニメーション名、テキスト生成時の情報
SupplyPoints			= {}
SupplyPoints.In			= "_05"
SupplyPoints.Out 		= "_06"
-- SupplyPoints.RotText 	= "_00"
-- SupplyPoints.OutText 	= "_02"
SupplyPoints.TextNode		= "Lsupply"
SupplyPoints.TextID		= "SUPPLY"
SupplyPoints.ScaleX		= 1.0
SupplyPoints.ScaleY		= 1.0
UseMultiModel( "PAUSE_Supply_00", SupplyPoints, 3 ) -- 五個作成

-- 武器マーカーのモデル名、アニメーション名、テキスト生成時の情報
WeaponPoints			= {}
WeaponPoints.In			= "_05"
WeaponPoints.Out 		= "_06"
-- WeaponPoints.RotText 	= "_00"
-- WeaponPoints.OutText 	= "_02"
WeaponPoints.TextNode		= "Lweapon"
WeaponPoints.TextID		= "WEAPON"
WeaponPoints.ScaleX		= 0.5
WeaponPoints.ScaleY		= 0.5
UseMultiModel( "PAUSE_Weapon_00", WeaponPoints, 10 ) -- 五個作成

-- 撤退マーカーのモデル名、アニメーション名、テキスト生成時の情報
WithdrawPoints1			= {}
WithdrawPoints1.In		= "_00"
WithdrawPoints1.Out 		= "_02"
-- WithdrawPoints1.RotText 	= nil
-- WithdrawPoints1.OutText 	= nil
WithdrawPoints1.TextNode	= nil
WithdrawPoints1.TextID		= nil
WithdrawPoints1.ScaleX		= 1.0
WithdrawPoints1.ScaleY		= 1.0
UseMultiModel( "PAUSE_Escape0_00", WithdrawPoints1, 2 ) -- ２個作成

-- 撤退マーカーのモデル名、アニメーション名、テキスト生成時の情報
WithdrawPoints2			= {}
WithdrawPoints2.In		= nil
WithdrawPoints2.Out 		= nil
-- WithdrawPoints2.RotText 	= "_00"
-- WithdrawPoints2.OutText 	= "_02"
WithdrawPoints2.TextNode	= "Lescape"
WithdrawPoints2.TextID		= "ESCAPE"
WithdrawPoints2.ScaleX		= 1.0
WithdrawPoints2.ScaleY		= 1.0
UseMultiModel( "PAUSE_Escape1_00", WithdrawPoints2, 2 ) -- ２個作成

-- 僚機マーカーのモデル名、アニメーション名、テキスト生成時の情報
BrotherPoints			= {}
BrotherPoints.In		= "_05"
BrotherPoints.Out 		= "_06"
-- BrotherPoints.RotText 	= "_00"
-- BrotherPoints.OutText 	= "_02"
BrotherPoints.TextNode		= "Lunit"
BrotherPoints.TextID		= "WING"
BrotherPoints.ScaleX	= 1.0
BrotherPoints.ScaleY	= 1.0
UseMultiModel( "PAUSE_UnitW_00", BrotherPoints, 2 ) -- ２個作成


angle = 0.0




-- 毎回ループ部分
function Update()

end

-- マップ決定時
function inEvent.MAP_DECIDE_TRIG()
	Message( "MAP_SELECT" )
end

-- マップ決定中の処理
function loopEvent.MAP_DECIDE_TRIG()
	x, y = GetMapZoomPos()
	Draw( MapMaker.Name, 0.0, 0.0, 0.0, false, x, y )
end

-- マップ決定解除処理
function outEvent.MAP_DECIDE_TRIG()
	Message( "MAP_UNSELECT" )
end

-- マップのズーム開始時
function inEvent.MAP_ZOOM_TRIG()
	Message( "MAP_ZOOMIN" )
end

-- マップのズームアウト時の処理
function outEvent.MAP_ZOOM_TRIG()
	Message( "MAP_ZOOMOUT" )
end

-- マップ選択中の処理
function loopEvent.MAP_SELECT_TRIG()

	-- マップのポイント描画
	DrawMapSelf( OwnPoint )

	DrawMapPoints( EnemyPoints, 	"EnemyMS" 	)
	DrawMapPoints( AttackPoints,	"AttackTG"	)
	DrawMapPoints( FriendPoints, 	"FriendMS" 	)
	DrawMapPoints( BrotherPoints, 	"BrotherMS" 	)
	DrawMapPoints( DefensePoints, 	"DefenceTG" 	)
	DrawMapPoints( SupplyPoints, 	"Supply" 	)
	DrawMapPoints( WeaponPoints, 	"Weapon" 	)
	DrawMapPoints( WithdrawPoints1, "WithDraw"	)
	DrawMapPoints( WithdrawPoints2, "WithDraw"	)


end

-- マップリセット時の処理
function MapReset()
	ResetMapSelf( OwnPoint )
	ResetMapPoints( EnemyPoints	)
	ResetMapPoints( AttackPoints 	)
	ResetMapPoints( FriendPoints 	)
	ResetMapPoints( BrotherPoints	)
	ResetMapPoints( DefensePoints 	)
	ResetMapPoints( SupplyPoints 	)
	ResetMapPoints( WeaponPoints 	)
	ResetMapPoints( WithdrawPoints1 )
	ResetMapPoints( WithdrawPoints2 )


end

-- マップ閉じる時の処理
function MapClose()
	RemoveMapSelf( OwnPoint )

	RemoveMapPoints( EnemyPoints	)
	RemoveMapPoints( AttackPoints 	)
	RemoveMapPoints( FriendPoints 	)
	RemoveMapPoints( BrotherPoints	)
	RemoveMapPoints( DefensePoints 	)
	RemoveMapPoints( SupplyPoints 	)
	RemoveMapPoints( WeaponPoints 	)
	RemoveMapPoints( WithdrawPoints1 )
	RemoveMapPoints( WithdrawPoints2)

end

function MapTextClose()
	CloseText( EnemyPoints, 	"EnemyMS" 	)
	CloseText( AttackPoints,	"AttackTG"	)
	CloseText( FriendPoints, 	"FriendMS" 	)
	CloseText( BrotherPoints,	"BrotherMS"	)
	CloseText( DefensePoints, 	"DefenceTG" 	)
	CloseText( SupplyPoints, 	"Supply" 	)
	CloseText( WeaponPoints, 	"Weapon" 	)
	CloseText( WithdrawPoints2, 	"WithDraw" 	)
end

-- マップのズーム完了時の処理
function inEvent.MAP_ZOOM_COMPLETE_TRIG()
	ShowText( EnemyPoints, 		"EnemyMS" 	)
	ShowText( AttackPoints,		"AttackTG"	)
	ShowText( FriendPoints, 	"FriendMS" 	)
	ShowText( BrotherPoints,	"BrotherMS"	)
	ShowText( DefensePoints, 	"DefenceTG" 	)
	ShowText( SupplyPoints, 	"Supply" 	)
	ShowText( WeaponPoints, 	"Weapon" 	)
	ShowText( WithdrawPoints2, 	"WithDraw" 	)

end

-- マップのズーム完了から抜けるときの処理
function outEvent.MAP_ZOOM_COMPLETE_TRIG()
	CloseText( EnemyPoints, 	"EnemyMS" 	)
	CloseText( AttackPoints,	"AttackTG"	)
	CloseText( FriendPoints, 	"FriendMS" 	)
	CloseText( BrotherPoints,	"BrotherMS"	)
	CloseText( DefensePoints, 	"DefenceTG" 	)
	CloseText( SupplyPoints, 	"Supply" 	)
	CloseText( WeaponPoints, 	"Weapon" 	)
	CloseText( WithdrawPoints2, 	"WithDraw" 	)


end



-- ステータスの自機表示開始の処理
function inEvent.MY_MS_SELECT_TRIG()
	Message( "PLAYER_S_ON" )
	SetReplaceText("FIGHT_WEAPON_NAME", FIGHT_NONAME )
	SetReplaceText("SHOT_WEAPON_NAME", SHOT_NONAME )
	SetReplaceText("SUB1_NAME", SUB1_NONAME )
	SetReplaceText("SUB2_NAME", SUB2_NONAME )
end

-- ステータスの右側表示開始の処理
function inEvent.BROTHER_TWO_SELECT_TRIG()
	Message( "PLAYERr_S_ON" )
	SetReplaceText("FIGHT_WEAPON_NAME", FIGHT_NONAME )
	SetReplaceText("SHOT_WEAPON_NAME", SHOT_NONAME )
	SetReplaceText("SUB1_NAME", SUB1_NONAME )
	SetReplaceText("SUB2_NAME", SUB2_NONAME )
	
end

-- ステータスの左側表示開始の処理
function inEvent.BROTHER_ONE_SELECT_TRIG()
	Message( "PLAYERl_S_ON" )
	SetReplaceText("FIGHT_WEAPON_NAME", FIGHT_NONAME )
	SetReplaceText("SHOT_WEAPON_NAME", SHOT_NONAME )
	SetReplaceText("SUB1_NAME", SUB1_NONAME )
	SetReplaceText("SUB2_NAME", SUB2_NONAME )

end

-- ステータスの左側に移動するときの処理
function inEvent.STATUS_LEFT_MOVE_TRIG()
	Message( "Triangle_Ls" )
end

-- ステータスの左側に移動した後の処理
function outEvent.STATUS_LEFT_MOVE_TRIG()
	Message( "Triangle_rL" )
end

-- ステータスの右側に移動するときの処理
function inEvent.STATUS_RIGHT_MOVE_TRIG()
	Message( "Triangle_Rs" )
end

-- ステータスの右側に移動した後の処理
function outEvent.STATUS_RIGHT_MOVE_TRIG()
	Message( "Triangle_rR" )
end

-- リトライ選択時の処理
function inEvent.RETRY_PRE_DECIDE_TRIG()
	Sound( SE_WINDOW_OPEN )
	Message( "MULTI_WINDOW_IN_RETRY" )
end

-- リトライ選択から抜けるときの処理
function outEvent.RETRY_PRE_DECIDE_TRIG()
	Sound( SE_WINDOW_CLOSE )
	Message( "MULTI_WINDOW_OUT_RETRY" )
end

-- 戻る選択時の処理
function inEvent.RETURN_PRE_DECIDE_TRIG()
	Sound( SE_WINDOW_OPEN )
	Message( "MULTI_WINDOW_IN_RETURN" )
end

-- 戻る選択から抜けるときの処理
function outEvent.RETURN_PRE_DECIDE_TRIG()
	Sound( SE_WINDOW_CLOSE )
	Message( "MULTI_WINDOW_OUT_RETURN" )
end

-- ゲーム終えるときの処理
function inEvent.QUIT_PRE_DECIDE_TRIG()
	Sound( SE_WINDOW_OPEN )
	Message( "MULTI_WINDOW_IN_QUIT" )
end

-- ゲーム終えるの決定を抜ける時の処理
function outEvent.QUIT_PRE_DECIDE_TRIG()
	Sound( SE_WINDOW_CLOSE )
	Message( "MULTI_WINDOW_OUT_QUIT" )
end


-- ＹＥＳにカーソルが選択されたときの処理
function inEvent.SELECT_YES_TRIG()
	Message( "MULTI_WINDOW_YES_ON" )
end

-- ＹＥＳに決定したときの処理
function inEvent.DICIDE_YES_TRIG()
	Message( "MULTI_WINDOW_YES_CALL" )
end


-- ＮＯにカーソルが選択されたときの処理
function inEvent.SELECT_NO_TRIG()
	Message( "MULTI_WINDOW_NO_ON" )
end

-- ＮＯにカーソル選択を抜けたときの処理
function inEvent.DICIDE_NO_TRIG()
	Message( "MULTI_WINDOW_NO_CALL" )
end




-- ステータス選択中の処理
function loopEvent.STATUS_SELECT_TRIG()
	Message("STATUS_HP", GetLifeRate())
	Message("STATUS_RELOAD", GetReloadRate())
	Message("STATUS_SHOOT", GetShootPowerRate() )
end

-- ミッションの単数行選択時の処理
function inEvent.MISSION_SINGLE_LINE_TRIG()
 	Message("MISSION_BUTTON_INF1")
end

-- ミッションの複数行選択時の処理
function outEvent.MISSION_SINGLE_LINE_TRIG()
 	-- Message("MISSION_BUTTON_INF1")
end

-- ミッションの複数行選択矢印表示開始処理
function inEvent.MISSION_MULTI_LINE_TRIG()

 	Message("Triangle_L")
 	Message("Triangle_R")
 	Message("MISSION_BUTTON_INF2")
end

-- ミッションの複数行矢印表示終了処理
function outEvent.MISSION_MULTI_LINE_TRIG()
  	Message("Triangle_rL")
 	Message("Triangle_rR")
end

-- マニュアル表示処理
function loopEvent.MANUAL_SELECT_TRIG()

	if PauseTrigger( "MANUAL_PAGE_NEXT_TRIG" ) then
		Message( MANUAL_CONTENT[ ManualPage ].OFF_MSG )
		NextManual()
		Message( MANUAL_CONTENT[ ManualPage ].ON_MSG )
	end

	if PauseTrigger( "MANUAL_PAGE_PRIV_TRIG" ) then
		Message( MANUAL_CONTENT[ ManualPage ].OFF_MSG )
		PrevManual()
		Message( MANUAL_CONTENT[ ManualPage ].ON_MSG )
	end
end

-- ビーム使用可能処理
function inEvent.BEAM_USE_TRIG()
	Message( "BEAM_Y_ON" )
end

-- 両手武器使用可能処理
function inEvent.DOUBLE_USE_TRIG()
	Message( "DOUBLE_Y_ON" )
	
end

-- 肩装備使用可能処理
function inEvent.SHOULDER_USE_TRIG()
	Message( "SHOULDER_Y_ON" )

end

-- ビーム使用可能アウト処理
function outEvent.BEAM_USE_TRIG()
	Message( "BEAM_Y_OFF" )

end

-- 両手武器使用可能アウト処理
function outEvent.DOUBLE_USE_TRIG()
	Message( "DOUBLE_Y_OFF" )

end

-- 肩装備使用可能アウト処理
function outEvent.SHOULDER_USE_TRIG()
	Message( "SHOULDER_Y_OFF" )
end

-- ビーム使用使用不可能処理
function inEvent.BEAM_UNUSE_TRIG()
	Message( "BEAM_N_ON" )
end

-- 両手武器使用不可能処理
function inEvent.DOUBLE_UNUSE_TRIG()
	Message( "DOUBLE_N_ON" )
end

-- 肩武器使用不可能処理
function inEvent.SHOULDER_UNUSE_TRIG()
	Message( "SHOULDER_N_ON" )

end

-- ビーム兵器使用不可能開放処理
function outEvent.BEAM_UNUSE_TRIG()
	Message( "BEAM_N_OFF" )

end

-- 両手武器使用不可能開放処理
function outEvent.DOUBLE_UNUSE_TRIG()
	Message( "DOUBLE_N_OFF" )

end

-- 肩武器使用不可能開放処理
function outEvent.SHOULDER_UNUSE_TRIG()
	Message( "SHOULDER_N_OFF" )
end

-- ＯＫサウンド処理
function inEvent.OK_SOUND_TRIG()
	Sound( "OK" )
end


-- キャンセルサウンド処理
function inEvent.CANCEL_SOUND_TRIG()
	Sound( "Cancel" )
end

-- カーソルサウンド処理
function inEvent.CURSOR_SOUND_TRIG()
	Sound( "Cursor" )
end


descriptionTextSet = {
	{
		text_name = "MISSION",
		link_model = nil,
		link_name = nil,
	}
}

xx = "blaaaah"
--SetReplaceText("MISSION", xx)