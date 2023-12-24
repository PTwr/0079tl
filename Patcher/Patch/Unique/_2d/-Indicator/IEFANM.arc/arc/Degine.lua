




WARNING_LIFE_RATE 	= 20.0			-- 警告ＨＰ
WARN_TIME_FRAME		= 3000.0		-- 警告フレーム数
EXAM_VOICE 		= "exm01"		-- EXAM開始ボイス
EXAM_END_VOICE 		= "exm02"		-- EXAM終了ボイス
isExam 			= false			-- EXAM声呼び出し中チェック
SCALE_EXAM_TIME 	= 999.0 / 10800.0	-- EXAMの時間換算
isExamWarning		= false			-- EXAM警告フラグ（ＥＸＡＭ起動時にオン）
-- EXAM_WARNING_FRAME	= 100.0 / SCALE_EXAM_TIME	-- EXAM警告フレーム
EXAM_WARNING_FRAME	= 1800.0		-- EXAM警告フレーム

--margins for reticle movement
RETICLE_MARGIN_SIDE	= 0.8			-- レティクル横幅割合
RETICLE_MARGIN_TOP	= 0.9			-- レティクル上の割合
RETICLE_MARGIN_BOTTOM	= 0.85			-- レティクル下の割合

--brsar sound codes
SE_FORMATION_CHG	= "Digital"		-- フォーメーション変更音
SE_OVERHEAT		= "Warning02"		-- オーバーヒート音
SE_SUPPLY		= "MSStartUp"		-- 補給中音（微妙）
SE_WARNING_HP		= "Warning03"		-- ＨＰ警告音
SE_WEAPON_CHG		= "Switch"		-- 武器切り替え音
SE_CONFRONT_OFF		= "Reload"		-- 格闘状態オフ
SE_EXAM_TIMER_START	= "OK"			-- EXAMタイマー開始
SE_EXAM_TIMER_END	= "Exhaust"		-- EXAMタイマー終了
SE_TIMER_START		= "OK"			-- タイマー開始
SE_WARNING_TIMER	= "Warning03"		-- タイマー警告
SE_WARNING_EXAM_TIMER	= "Warning03"		-- EXAMタイマー警告音


-- モデルのExpand処理
AttachModel("HP_gauge_00", 	"HP_BAR_00", 		"HP_BAR_00")
AttachModel("Vernier_gauge_00", "Vernier_BAR_00", 	"Vernier_BAR_00")
AttachModel("Revo_RED_BLUE_00", "Revo_SPARC_00", 	"Revo_SPARC_00")



-- 敵ＭＳユニットマーカー
Enemy_MS_Makers			= {}
Enemy_MS_Makers.InAnim		= "_00"	-- 表示アニメーション名
Enemy_MS_Makers.ShowAnim	= "_01" -- 表示中のアニメーション名
Enemy_MS_Makers.OutAnim		= "_02" -- 消去アニメーション名
Enemy_MS_Makers.InReticleAnim	= "_05" -- レティクル中アニメーション名
Enemy_MS_Makers.OutReticleAnim	= "_01" -- レティクル消去アニメーション名
Enemy_MS_Makers.ShowFrame 	= 100.0 -- 半透明変更フレーム数
UseMultiModel( "LOCK_ON_Maker1_00", Enemy_MS_Makers, 10 ) -- 五個作成



-- 味方ＭＳユニットマーカー
Friend_MS_Makers		= {}
Friend_MS_Makers.InAnim		= "_00"	-- 表示アニメーション名
Friend_MS_Makers.ShowAnim	= "_01" -- 表示中のアニメーション名
Friend_MS_Makers.OutAnim	= "_02" -- 消去アニメーション名
Friend_MS_Makers.InReticleAnim	= "_05" -- レティクル中アニメーション名
Friend_MS_Makers.OutReticleAnim	= "_01" -- レティクル消去アニメーション名
Friend_MS_Makers.ShowFrame 	= 100.0 -- 半透明変更フレーム数
UseMultiModel( "LOCK_ON_Maker2_00", Friend_MS_Makers, 10 ) -- 五個作成

-- 格闘可能マーカー
Confront_Markers		= {}
Confront_Markers.InAnim		= "_00" -- 表示アニメーション
Confront_Markers.OutAnim	= "_02" -- 消去アニメーション
Confront_Markers.InReticleAnim	= "_05"	-- レティクル中表示アニメーション
Confront_Markers.OutReticleAnim	= "_00" -- レティクル消去アニメーション
UseMultiModel( "Confront_Marker_00", Confront_Markers, 10 )

-- 交換不可能武器マーカー
Waepon_Makers_N 			= {}
Waepon_Makers_N.InDistance 		= 300.0	-- 表示距離
Waepon_Makers_N.OutAreaInAnim		= "_00"	-- 表示アニメーション名
Waepon_Makers_N.InAreaInAnim		= "_01" -- 交換可能距離の表示中のアニメーション名
Waepon_Makers_N.InAreaLoopAnim		= "_02"	-- 交換可能距離の表示中のループアニメ
Waepon_Makers_N.InAreaOutAnim		= "_03" -- 交換可能距離から離れた時の表示中のアニメーション名
Waepon_Makers_N.OutAreaOutAnim		= "_04" -- 消去アニメーション名
Waepon_Makers_N.InAreaReticleAnim	= "_05" -- レティクル中アニメーション名
Waepon_Makers_N.OutAreaReticleAnim	= "_10" -- レティクル中交換可能外アニメーション名
UseMultiModel( "Waepon_Maker_N_00", Waepon_Makers_N, 10 )

-- 交換可能武器マーカー
Waepon_Makers				= {}
Waepon_Makers.InDistance	 	= 300.0	-- 表示距離
Waepon_Makers.OutAreaInAnim		= "_00"	-- 表示アニメーション名
Waepon_Makers.InAreaInAnim		= "_01" -- 交換可能距離の表示中のアニメーション名
Waepon_Makers.InAreaLoopAnim		= "_02"	-- 交換可能距離の表示中のループアニメ
Waepon_Makers.InAreaOutAnim		= "_03" -- 交換可能距離から離れた時の表示中のアニメーション名
Waepon_Makers.OutAreaOutAnim		= "_04" -- 消去アニメーション名
Waepon_Makers.InAreaReticleAnim		= "_05" -- レティクル中アニメーション名
Waepon_Makers.OutAreaReticleAnim	= "_10" -- レティクル中交換可能外アニメーション名
UseMultiModel( "Waepon_Maker_00", Waepon_Makers, 10 )

-- 補給可能コンテナマーカー
Supply_Makers				= {}
Supply_Makers.InDistance 		= 300.0	-- 表示距離
Supply_Makers.OutAreaInAnim		= "_00"	-- 表示アニメーション名
Supply_Makers.InAreaInAnim		= "_01" -- 交換可能距離の表示中のアニメーション名
Supply_Makers.InAreaLoopAnim		= "_02"	-- 交換可能距離の表示中のループアニメ
Supply_Makers.InAreaOutAnim		= "_03" -- 交換可能距離から離れた時の表示中のアニメーション名
Supply_Makers.OutAreaOutAnim		= "_04" -- 消去アニメーション名
Supply_Makers.InAreaReticleAnim		= "_05" -- レティクル中アニメーション名
Supply_Makers.OutAreaReticleAnim	= "_10" -- レティクル中補給可能外アニメーション名
UseMultiModel( "Supply_Maker_00", Supply_Makers, 10 )

-- レーダーのバー作成処理
bar = {}
UseMultiModel( "Radar_BAR_A_00", bar, 1 )
UseMultiModel( "Radar_BAR_B_00", bar, 1 )

--minimap red frame and points on it, blinky background is just background
-- レーダー配置情報
Rader			= {}
Rader.CenterX 		= -24.0		-- 中心Ｘ座標
Rader.CenterY 		= -16.077	-- 中心Ｙ座標
Rader.Width 		= 12.0		-- 中心横幅
Rader.Height		= 12.0		-- 中心高さ
Rader.Range		= 1000.0	-- レーダーの索敵範囲



-- 敵MSのレーダー用ポイント
Rader_points_A 		= {}
Rader_points_A.InAnim	= "_00"		-- インアニメーション
Rader_points_A.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_A_00", Rader_points_A, 15 )

-- 攻撃目標用のレーダー用ポイント
Rader_points_B 		= {}		
Rader_points_B.InAnim	= "_00"		-- インアニメーション
Rader_points_B.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_B_00", Rader_points_B, 10 )

-- 味方ＭＳのレーダー用ポイント
Rader_points_C 		= {}
Rader_points_C.InAnim	= "_00"		-- インアニメーション
Rader_points_C.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_C_00", Rader_points_C, 8 )

-- 防衛目標のレーダー用ポイント
Rader_points_D 		= {}
Rader_points_D.InAnim	= "_00"		-- インアニメーション
Rader_points_D.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_D_00", Rader_points_D, 10 )

-- 僚機のレーダー用ポイント
Rader_points_E 		= {}
Rader_points_E.InAnim	= "_00"		-- インアニメーション
Rader_points_E.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_E_00", Rader_points_E, 2 )

-- 武器のレーダー用ポイント
Rader_points_F 		= {}
Rader_points_F.InAnim	= "_00"		-- インアニメーション
Rader_points_F.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_F_00", Rader_points_F, 7 )

-- 補給ポイントのレーダー用ポイント
Rader_points_G 		= {}
Rader_points_G.InAnim	= "_00"		-- インアニメーション
Rader_points_G.OutAnim	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_G_00", Rader_points_G, 3 )

-- 撤退ポイントのレーダー用ポイント
Rader_points_H		= {}
Rader_points_H.InAnim	= "_00"
Rader_points_H.OutAnim	= "_02"
UseMultiModel( "Z_Rader_point_H_00", Rader_points_H, 2 )


--hud direction points (top of scope)
-- 方位系のレーダー配置情報
Radar_Dir		= {}		
Radar_Dir.CenterY	= 15.35		-- 表示のＹ座標
Radar_Dir.Width		= 6.5		-- 表示する横幅
Radar_Dir.Range		= 3000.0	-- 表示する範囲
Radar_Dir.Angle		= 90.0		-- 表示する角度

-- 敵ＭＳの方位系用ポイント
Dir_points_A 		= {}
Dir_points_A.InAnim 	= "_00"		-- インアニメーション
Dir_points_A.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_A_00", Dir_points_A, 15 )

-- 攻撃目標の方位系用ポイント
Dir_points_B 		= {}		
Dir_points_B.InAnim 	= "_00"		-- インアニメーション
Dir_points_B.ShowAnim	= "_01"		-- 表示中アニメーション
Dir_points_B.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_B_00", Dir_points_B, 10 )

-- 味方ＭＳの方位系用ポイント
Dir_points_C = {}
Dir_points_C.InAnim 	= "_00"		-- インアニメーション
Dir_points_C.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_C_00", Dir_points_C, 8 )

-- 防衛目標の方位系用ポイント
Dir_points_D 		= {}
Dir_points_D.InAnim 	= "_00"		-- インアニメーション
Dir_points_D.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_D_00", Dir_points_D, 10 )

-- 僚機の方位系用ポイント
Dir_points_E = {}
Dir_points_E.InAnim 	= "_00"		-- インアニメーション
Dir_points_E.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_E_00", Dir_points_E, 2 )

-- 補給の方位系用ポイント
Dir_points_G = {}
Dir_points_G.InAnim 	= "_00"		-- インアニメーション
Dir_points_G.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Rader_point_G_00", Dir_points_G, 3 )

-- 撤退の方位系用ポイント
Dir_points_H = {}
Dir_points_H.InAnim 	= "_00"		-- インアニメーション
Dir_points_H.OutAnim 	= "_02"		-- 消去アニメーション
UseMultiModel( "Z_Rader_point_H_00", Dir_points_H, 2 )

-- 毎回ループ部分
function Update()

	-- 常に表示される計器系
	Message( "HP_Gauge_100F", 	GetLifeRate(), true )
	Message( "VR_Gauge_100F", 	GetThrustorRate(), true)

	
end


-- レーダー表示中
function DrawRader()
	-- レーダーのポイント描画
	DrawRaderPoints( Rader, Rader_points_A, "EnemyMS" 	)
	DrawRaderPoints( Rader, Rader_points_B, "AttackTG"	)
	DrawRaderPoints( Rader, Rader_points_C, "FriendMS" 	)
	DrawRaderPoints( Rader, Rader_points_D, "DefenceTG" 	)
	DrawRaderPoints( Rader, Rader_points_E, "BrotherMS" 	)
	DrawRaderPoints( Rader, Rader_points_F, "Weapon" 	)
	DrawRaderPoints( Rader, Rader_points_G, "Supply" 	)
	DrawRaderPoints( Rader, Rader_points_H, "WithDraw" 	)

end

-- レーダーの表示アウト
function DrawRaderOut()
	RemovePoints( Rader_points_A )
	RemovePoints( Rader_points_B )
	RemovePoints( Rader_points_C )
	RemovePoints( Rader_points_D )
	RemovePoints( Rader_points_E )
	RemovePoints( Rader_points_F )
	RemovePoints( Rader_points_G )
	RemovePoints( Rader_points_H )
end

-- 方位系表示中
function DrawDir()
	-- 方位系のポイント描画
	DrawDirPoints(	Radar_Dir, Dir_points_A, "EnemyMS")
	DrawDirPoints(	Radar_Dir, Dir_points_B, "AttackTG")
	DrawDirPoints(	Radar_Dir, Dir_points_C, "FriendMS")
	DrawDirPoints(	Radar_Dir, Dir_points_D, "DefenceTG")
	DrawDirPoints(	Radar_Dir, Dir_points_E, "BrotherMS")
	DrawDirPoints(	Radar_Dir, Dir_points_G, "Supply")
	DrawDirPoints(	Radar_Dir, Dir_points_H, "WithDraw")

end

-- 方位系表アウト
function DrawDirOut()
	RemovePoints( Dir_points_A )
	RemovePoints( Dir_points_B )
	RemovePoints( Dir_points_C )
	RemovePoints( Dir_points_D )
	RemovePoints( Dir_points_E )
	RemovePoints( Dir_points_G )
	RemovePoints( Dir_points_H )
end

-- マーカー表示中
function DrawMaker()
	-- 新マーカー表示処理
	confDistance = GetConfrontArea()
	lockDistance = GetLockOnArea()
	Waepon_Makers.InDistance 	= lockDistance
	Waepon_Makers_N.InDistance 	= lockDistance
	Supply_Makers.InDistance 	= lockDistance
	DrawMakerNF( Enemy_MS_Makers, "EnemyN", confDistance, lockDistance  )
	DrawMakerNF( Friend_MS_Makers, "FriendN", 0, lockDistance )
	DrawMakerNF( Confront_Markers, "EnemyC", 0, confDistance )
	DrawAreaMaker( Waepon_Makers, "Weapon_P", GetWeaponPickUpArea())
	DrawAreaMaker( Waepon_Makers_N, "Weapon_IP", GetWeaponPickUpArea())
	DrawAreaMaker( Supply_Makers, "SupplyMaker", GetSupplyArea() )

end

-- マーカー表示アウト
function DrawMakerOut()
	RemoveNFTarget( Enemy_MS_Makers )
	RemoveNFTarget( Friend_MS_Makers )
	RemoveNFTarget( Confront_Markers )
	RemoveAreaTarget( Waepon_Makers )
	RemoveAreaTarget( Waepon_Makers_N )
	RemoveAreaTarget( Supply_Makers )
end




-- ＨＰピンチ状態でスナイプから通常状態に遷移するとき
function WarningNormalIn()
	Message( "Scope_R_Ind_fadeIn" )
end

-- ＨＰ通常状態でスナイプから通常状態に遷移するとき
function NormalIn()
	Message("Scope_Y_Ind_fadeIn" )
end

-- 警告時の更新処理
function WarningUpdate()
	Message("Warning_BRG_Gauge", 		GetDirFrame() )
end


-- 非警告時の更新処理
function UnwarningUpdate()
	Message("BRG_Gauge", 			GetDirFrame() )
end

-- 射撃モードイン
function ShootModeIn()
	-- Message( "Main_Dig_ON")
end

-- 射撃モード中
function ShootMode()
	Message( "Shooter_Gauge_100F",  GetHeatRate())
	posX, posY, posZ = GetPosition()
	Message( "Main_Gauge_Shooting", posY )

end

-- ＨＰピンチ時の射撃モード
function WarnShootMode()
	Message( "Shooter_Gauge_100F",  GetHeatRate())
	posX, posY, posZ = GetPosition()
	Message( "Warning_Main_Gauge_Shooting", posY )	
end

-- 格闘モードイン
function FightModeIn()
	Message( "Main_Gauge_Fight_IN")
	-- Message( "Main_Dig_OFF")
	Anim( "Main_Dig_00", 		"_01", 50.0 )
	Anim( "Main_Dig_10_00", 	"_01", 50.0 )
	Anim( "Main_Dig_100_00", 	"_01", 50.0 )

	DrawOnTarget( "Confront_Lock_00", 	GetLockOnPos() )
	DrawOnTarget( "ENEMY_HPS_00", 		GetLockOnPos() )
	DrawOnTarget( "TARGET_HPS_00", 		GetLockOnPos() )
end

-- 格闘モードアウト
function FightModeOut()

	Message( "Main_Gauge_Fight_OUT")
	Message( "ENEMY_HPS_OUT", 30.0 )
	DrawOnTarget( "ENEMY_HPS_00", 		GetLockOnPos() )
	Sound( SE_CONFRONT_OFF )
end

-- 警告中格闘モードイン
function WarnFightModeIn()
	Message( "Warning_Main_Gauge_Fight_IN")
	-- Message( "Main_Dig_OFF")
	Anim( "Main_Dig_00", 		"_01", 50.0 )
	Anim( "Main_Dig_10_00", 	"_01", 50.0 )
	Anim( "Main_Dig_100_00", 	"_01", 50.0 )

	DrawOnTarget( "Confront_Lock_00", 	GetLockOnPos() )
	DrawOnTarget( "ENEMY_HPS_00", 		GetLockOnPos() )
	DrawOnTarget( "TARGET_HPS_00", 		GetLockOnPos() )
end

-- 警告中格闘モードアウト
function WarnFightModeOut()
	Message( "ENEMY_HPS_OUT", 30.0 )
	Message( "Warning_Main_Gauge_Fight_OUT")
	DrawOnTarget( "ENEMY_HPS_00", 		GetLockOnPos() )
	Sound( SE_CONFRONT_OFF )
end



-- 格闘モード中
function FightMode()
	Message( "Confront_Lock_200F")
	Message( "ENEMY_HPS_100F", GetLockOnLifeRate() )
	Message( "Shooter_Gauge_100F",  GetGuardSaberRate())

	DrawOnTarget( "Confront_Lock_00", 	GetLockOnPos() )
	DrawOnTarget( "ENEMY_HPS_00", 		GetLockOnPos() )
	DrawOnTarget( "TARGET_HPS_00", 		GetLockOnPos() )
end



-- 補給中イン
function SupplyWaitIn()
	StartSE( SE_SUPPLY )
	Message( "Supply_Panel_OPEN" )
end


-- 補給中アウト
function SupplyWaitOut()
	StopSE( SE_SUPPLY )
	Message( "Supply_Panel_OUT" )
end


-- 敵レティクルイン
function ReticleEnemyIn()
	-- Message("ENEMY_HP_IN")
	DrawOnTarget( "ENEMY_HP_00", 	GetReticleEnemyPos() )
end

-- 敵レティクル中
function ReticleEnemy()
	Message("ENEMY_HP_100F", GetReticleEnemyLifeRate() )
	DrawOnTarget( "ENEMY_HP_00", 	GetReticleEnemyPos() )
end

-- 敵レティクルアウト
function ReticleEnemyOut()
	Message("ENEMY_HP_OUT", 30.0 )
end

-- 味方レティクルイン
function ReticleFriendIn()
	Message("FRIEND_HP_100F", GetReticleFriendLifeRate() )
	DrawOnTarget( "FRIEND_HP_00", 	GetReticleFriendPos() )
end

-- 味方レティクル中
function ReticleFriend()
	Message("FRIEND_HP_100F", GetReticleFriendLifeRate() )
	DrawOnTarget( "FRIEND_HP_00", 	GetReticleFriendPos() )
end

-- 味方レティクルアウト
function ReticleFriendOut()
	Message("FRIEND_HP_OUT", 30.0 )
end

-- ターゲットレティクルイン
function ReticleTargetIn()
	Message("TARGET_HP_100F", GetReticleTargetLifeRate() )
	DrawOnTarget( "TARGET_HP_00", 	GetReticleTargetPos() )
end

-- ターゲットレティクル中
function ReticleTarget()
	Message("TARGET_HP_100F", GetReticleTargetLifeRate() )
	DrawOnTarget( "TARGET_HP_00", 	GetReticleTargetPos() )
end

-- ターゲットレティクルアウト
function ReticleTargetOut()
	Message("TARGET_HP_OUT" , 30.0)
end

-- レティクル取得可能武器イン
function ReticleChangeWeaponIn()
	Message("Waepon_CHG_OPEN")	
end

-- レティクル取得可能武器アウト
function ReticleChangeWeaponOut()
	Message("Waepon_CHG_OUT")	
end

-- レティクル取得不可能武器イン
function ReticleNoChangeWeaponIn()
	Message("Waepon_CHG_N_OPEN")	
end

-- レティクル取得不可能武器アウト
function ReticleNoChangeWeaponOut()
	Message("Waepon_CHG_N_OUT")	
end

-- メインウェポン→格闘可能イン
function WeaponMainFightIn()
	Message("WEAPON_MAIN_Fight_IN")
end


-- メインウェポン→格闘可能アウト
function WeaponMainFightOut()
	Message("Weapon_MAIN_Fight_OUT")
end

-- サブ１ウェポン→格闘可能イン
function WeaponSub1FightIn()
	Message("Weapon_SUB1_Fight_IN")
end


-- サブ１ウェポン→格闘可能アウト
function WeaponSub1FightOut()
	Message("Weapon_SUB1_Fight_OUT")
end

-- サブ２ウェポン→格闘可能イン
function WeaponSub2FightIn()
	Message("Weapon_SUB2_Fight_IN")
end


-- サブ２ウェポン→格闘可能イン
function WeaponSub2FightOut()
	Message("Weapon_SUB2_Fight_OUT")
end

-- 敵射程範囲外イン
function LockOnOutRangeIn()
	Message( "Range_TEX_OPEN")
end



-- 敵射程範囲外アウト
function LockOnOutRangeOut()
	Message( "Range_TEX_OUT")
end

-- 敵射程範囲内イン
function LockOnInRangeIn()
	Message( "Range_Lock_IN")
	DrawOnTarget( "Attack_Lock_B_00", 	GetLockOnPos() )
end

-- 敵射程範囲内中
function LockOnInRange()
	Message( "Range_Lock_100F", CalcPosParFrame( 0.0, GetLockOnArea(), GetLockOnDistance(), 100.0 ) )
	DrawOnTarget( "Attack_Lock_B_00", 	GetLockOnPos() )
end

-- 敵射程範囲内アウト
function LockOnInRangeOut()
	Message( "Range_Lock_OUT", 30.0 )
end

-- 敵をロックオンしたとき
function LockOnEnemyIn()
	Message( "Attack_Lock_E_IN")
end

-- 敵がロックオン距離中にいるとき
function LockOnEnemy()
	Message( "ENEMY_HPS_100F", GetLockOnLifeRate() )
	Message( "Attack_Lock_E_100F", CalcPosParFrame( 0.0, GetLockOnArea(), GetLockOnDistance(), 100.0 ))
	DrawOnTarget( "ENEMY_HPS_00", 		GetLockOnPos() )
	DrawOnTarget( "Attack_Lock_A_00", 	GetLockOnPos() )
end

-- 敵がロックオンから外れたとき
function LockOnEnemyOut()
	Message( "ENEMY_HPS_OUT", 30.0 )
	Message( "Attack_Lock_E_OUT", 30.0)
end

-- ターゲットがロックオンに入ったとき
function LockOnTargetIn()
	Message( "Attack_Lock_T_IN")
end

-- ターゲットがロックオンに入っているとき
function LockOnTarget()
	Message( "TARGET_HPS_100F", GetLockOnLifeRate() )
	Message( "Attack_Lock_T_100F", CalcPosParFrame( 0.0, GetLockOnArea(), GetLockOnDistance(), 100.0 ))
	DrawOnTarget( "TARGET_HPS_00", 		GetLockOnPos() )
	DrawOnTarget( "Attack_Lock_A_00", 	GetLockOnPos() )

end

-- ターゲットがロックオンから外れたとき
function LockOnTargetOut()
	Message( "TARGET_HPS_OUT", 30.0 )
	Message( "Attack_Lock_T_OUT", 30.0)
end

-- メイン武器イン
function WeaponMainIn()
	Message( "WEAPON_MAIN_IN")
end

-- 武器メインアウト
function WeaponMainOut()
	Sound( SE_WEAPON_CHG )
	Message( "WEAPON_MAIN_OUT")

end

-- サブ１武器イン
function WeaponSub1In()
	Message( "WEAPON_SUB1_IN")
end


-- サブ１武器アウト
function WeaponSub1Out()
	Sound( SE_WEAPON_CHG )
	Message( "WEAPON_SUB1_OUT")
end

-- サブ２武器イン
function WeaponSub2In()
	Message( "WEAPON_SUB2_IN")
end

-- サブ２武器アウト
function WeaponSub2Out()
	Sound( SE_WEAPON_CHG )
	Message( "WEAPON_SUB2_OUT")
end

-- メイン武器の空状態表示
function WeaponSelectMainEmptyIn()
	Message( "WEAPON_MAIN_RED")
end

-- メイン武器の空状態解除
function WeaponSelectMainEmptyOut()
	Message( "WEAPON_MAIN_LOOP")
end

-- ＳＵＢ１武器の空状態表示
function WeaponSelectSub1EmptyIn()
	Message( "WEAPON_SUB1_RED")
end

-- ＳＵＢ１武器の空状態解除
function WeaponSelectSub1EmptyOut()
	Message( "WEAPON_SUB1_LOOP")
end

-- ＳＵＢ２武器の空状態表示
function WeaponSelectSub2EmptyIn()
	Message( "WEAPON_SUB2_RED")
end

-- ＳＵＢ２武器の空状態解除
function WeaponSelectSub2EmptyOut()
	Message( "WEAPON_SUB2_LOOP")
end

-- 弾数設定
function SetBulletNum(n)
if (n==nil) then
n=GetBulletNum()
end
	Message( "Main_Dig_UV",  	n)
end

-- 鍔迫り合い開始
function DeadHeatIn()
	Message( "Revo_Bar_OPEN")
end

-- 鍔迫り合い中
function DeadHeat()
	Message( "Revo_Bar_100F", GetDeadHeatRate() )

end

-- 鍔迫り合い終了
function DeadHeatOut()
	Message( "Revo_Bar_OUT", 100.0)

end

-- BREAK表示処理
function WeaponBreakIn()
	Message( "WEAPON_GUARD_LOOP")
	Message( "WEAPON_BREAK_RED")
end

-- BREAK消去処理
function WeaponBreakOut()
	Message( "WEAPON_BREAK_OUT")
	Message( "WEAPON_GUARD_IN")
end


-- ビームレティクル表示開始
function BeamReticleIn()
	Message("Site_A_IN")
end

-- ビームレティクル表示中
function BeamReticle()
	-- レティクル描画
	Draw( 	"Site_A_00", 0.0, 0.0, 0.0, false, GetReticlePos() )
	Message( "Site_A_UV", GetReticleRate() )
end

-- レティクル消去
function BeamReticleOut()
	Message("Site_A_OUT")
end

-- 実弾レティクル表示開始
function BulletReticleIn()
	Message("Site_B_IN")
end

-- 実弾レティクル表示中
function BulletReticle()
	-- レティクル描画
	Draw( 	"Site_B_00", 0.0, 0.0, 0.0, false, GetReticlePos() )
	Message( "Site_B_UV", GetReticleRate() )
end

-- 実弾レティクル消去
function BulletReticleOut()
	Message("Site_B_OUT")
end

-- バズーカレティクル表示開始
function BazookaReticleIn()
	Message("Site_C_IN")
end

-- バズーカレティクル表示中
function BazookaReticle()
	-- レティクル描画
	Draw( 	"Site_C_00", 0.0, 0.0, 0.0, false, GetReticlePos() )
	Message( "Site_C_UV", GetReticleRate() )
end

-- バズーカレティクル消去
function BazookaReticleOut()
	Message("Site_C_OUT")
end

-- グレネードレティクル表示開始
function GrenadeReticleIn()
	Message("Site_D_IN")
end

-- グレネードレティクル表示中
function GrenadeReticle()
	-- レティクル描画
	Draw( 	"Site_D_00", 0.0, 0.0, 0.0, false, GetReticlePos() )
end

-- グレネードレティクル消去
function GrenadeReticleOut()
	Message("Site_D_OUT")
end

-- マルチロックレティクル表示
function MultiReticleIn()
	Message("Site_E_IN")
end

-- マルチロックレティクル表示中
function MultiReticle()
	-- レティクル描画
	Draw( 	"Site_E_00", 0.0, 0.0, 0.0, false, GetReticlePos() )
	Message( "Site_E_UV", 0.12 * GetReticleRate() )
end

-- マルチロックレティクル消去
function MultiReticleOut()
	Message("Site_E_OUT")
end

-- EXAM保持表示
function HasExamIn()
	Message("EXAM_Timer_IN" )
end

-- EXAM保持中の表示
function HasExam()
	Message("EXAM_Timer_UV", GetExamTime() * SCALE_EXAM_TIME)
end

-- EXAM保持中の消去（使われない）
function HasExamOut()
	Message("EXAM_Timer_OUT" )
end

-- EXAMボイス時の表示オン
function UseExamIn()
	Message("EXAM_SYSTEM_OPEN")

end

-- EXAMボイス時の表示アウト
function UseExamOut()
	Message("EXAM_SYSTEM_OUT")
end

-- タイマーの表示消去
function UseTimerOut()
	Message("TIME_Dig_OUT", 30.0 )
end

-- タイマー使用中
function UseTimer()
	Message("TIME_Dig_UV", GetIndicateTime() )
end

-- 警告時タイマー使用中
function UseWarnTimer()
	Message("RED_TIME_Dig_UV", GetIndicateTime() )
end

-- タイマー使用開始
function UseTimerIn()
	Message("TIME_Dig_IN")
	Sound( SE_TIMER_START )
end

-- 通常タイマー→警告時タイマー使用開始
function WarnTimerIn()
	Message("TIME_Dig_RED")
	Sound( SE_WARNING_TIMER )
end

-- 警告時タイマー→通常タイマー
function WarnTimerOut()
	Message("TIME_Dig_IN")
end

-- フォーメーションＡノーマル表示→非表示まで
function FormANIn()
	Message("Formation_A")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションＡオフェンス表示→非表示まで
function FormAOIn()
	Message("Formation_G")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションＡディフェンス表示→非表示まで
function FormADIn()
	Message("Formation_D")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションＶノーマル表示→非表示まで
function FormVNIn()
	Message("Formation_B")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションＶオフェンス表示→非表示まで
function FormVOIn()
	Message("Formation_H")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションＶディフェンス表示→非表示まで
function FormVDIn()
	Message("Formation_E")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションBreakノーマル表示→非表示まで
function FormBNIn()
	Message("Formation_C")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションBreakオフェンス表示→非表示まで
function FormBOIn()
	Message("Formation_I")
	Sound( SE_FORMATION_CHG )
end

-- フォーメーションBreakディフェンス表示→非表示まで
function FormBDIn()
	Message("Formation_F")
	Sound( SE_FORMATION_CHG )
end



-- オーバーヒートイン
function OverHeatIn()
	Message( "OVERHEAT_OPEN" )
	Sound( SE_OVERHEAT )
end

-- ＨＰ警告イン
function WarningLifeIn()
	Message( "Main_Gauge_RED_OPEN" )
	Sound( SE_WARNING_HP )
end


-- ロックオン状態イン
function LockOnIn()
	--Voice( "ain001" )
	--Sound( SE_OVERHEAT )
	Sound( "LockOn" )
end

-- ロックオン状態アウト
function LockOnOut()
	Sound( "LockOff" )
end

-- EXAM終了時
function ExamShutdown()
	Sound( SE_EXAM_TIMER_END )
	Voice( EXAM_END_VOICE )
end

--[[
function testReplacer(string)
	return "aaaBBBccc"
end
--load text stuff
descriptionTextSet[1].obj = AddText(man2, descriptionTextSet[1].text_name)
--register dynamic text value
SetTextReplacer(descriptionTextSet , "descReplacer")

--preposition textbox
Text_SetPos(descriptionTextSet[1].obj, 300, 300, -1)
--set it as visible, mech switcher will just change it to empty string when needed
Text_SetIsDisp(descriptionTextSet[1].obj, true)
Text_SetShadow(descriptionTextSet[1].obj, true)
]]