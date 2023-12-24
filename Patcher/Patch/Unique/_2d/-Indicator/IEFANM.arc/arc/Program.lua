-- 複数モデル作成のテーブル
multiCounter = 1

-- 複数モデルのテンポラリテーブル
multiCodeTable = {}

-- State値
NONE = 0


-- 武器・補給可能State値
SHOW 		= 1
IN_AREA		= 2
IN_AREA_LOOP	= 3
OUT_AREA	= 4
R_IN_AREA	= 5
R_OUT_AREA	= 6

-- そのほかレティクル表示のState値
ON_RETICLE	= 1
OFF_RETICLE	= 2


-- 複数作成モデルのプライオリティ一括設定関数
function SetMultiXluPriority( table, pri )
	for idx = 1, #table do
		SetXluPriority( table[idx].MdlName, pri )
	end
end

-- モデル表示処理（メインカメラからの座標により表示）
function DrawOnTarget( mdlName, x, y, z )
	Draw( mdlName, x, y, z, true)
end



-- 構築後に呼ばれる関数
function Regist()

	-- レーダーポイントのプライオリティ設定
	SetMultiXluPriority( Rader_points_F, 206 )
	SetMultiXluPriority( Rader_points_E, 207 )
	SetMultiXluPriority( Rader_points_G, 208 )
	SetMultiXluPriority( Rader_points_C, 209 )
	SetMultiXluPriority( Rader_points_A, 210 )
	SetMultiXluPriority( Rader_points_D, 211 )
	SetMultiXluPriority( Rader_points_B, 212 )
	SetMultiXluPriority( Rader_points_H, 213 )

	-- 方位系のプライオリティ設定
	SetMultiXluPriority( Dir_points_E, 207 )
	SetMultiXluPriority( Dir_points_G, 208 )
	SetMultiXluPriority( Dir_points_C, 209 )
	SetMultiXluPriority( Dir_points_A, 210 )
	SetMultiXluPriority( Dir_points_D, 211 )
	SetMultiXluPriority( Dir_points_B, 212 )
	SetMultiXluPriority( Dir_points_H, 213 )

	-- レーダーバーのプライオリティ設定
	SetXluPriority( "000Radar_BAR_A_00", 206 )
	SetXluPriority( "000Radar_BAR_B_00", 206 )
	SetXluPriority( "Radar_BAR_A_00", 206 )
	SetXluPriority( "Radar_BAR_B_00", 206 )

	-- 鍔迫り合い時の表示物のプライオリティ設定
	SetXluPriority( "Revo_con_00", 206 )
	SetXluPriority( "Revo_RED_BLUE_00", 206 )

	-- 武器パネルのプライオリティ設定
	SetXluPriority( "Weapon_Panel_00", 206 )

	-- レティクルの表示エリア割合設定
	SetReticleAreaMargin( RETICLE_MARGIN_TOP, RETICLE_MARGIN_BOTTOM, RETICLE_MARGIN_SIDE )

end

-- 複数モデルの作成後の登録関数
function SetMultiCodeTable( codeName, idx )
	multiCodeTable[multiCounter][idx] = {}
	multiCodeTable[multiCounter][idx].MdlName = codeName
	multiCodeTable[multiCounter][idx].IsReticle = false
	multiCodeTable[multiCounter][idx].IsShow = false
	multiCodeTable[multiCounter][idx].IsInArea = false
	multiCodeTable[multiCounter][idx].State = NONE
end

-- 複数モデルの作成関数
function UseMultiModel( mdlName, nameTable, multiNum )
	nameTable.MdlNum 	= multiNum
	nameTable.PreDrawNum 	= 0
	multiCodeTable[multiCounter] = nameTable
	RegistMultiModel( mdlName, multiNum )
	multiCounter = multiCounter + 1
end


voiceId = 999
n=0

-- インゲーム時のメインで流される関数
function ProgmaMain()

n=n+1

--voiceId =nil

--Voice Detection works!!! yaay
--Snd_VoiceGetPlay is not available here
--This loop is NOT executed during cutscenes, cutscene Voice was not detected
--but variable values remain so module is not reloaded
if IsPlayVoice("eve011") then
voiceId = 011
elseif IsPlayVoice("eve012") then
voiceId = 012
elseif IsPlayVoice("eve013") then
voiceId = 013
elseif IsPlayVoice("eve014") then
voiceId = 014
elseif IsPlayVoice("eve015") then
voiceId = 015
elseif IsPlayVoice("eve016") then
voiceId = 016
elseif IsPlayVoice("eve017") then
voiceId = 017
elseif IsPlayVoice("eve018") then
voiceId = 018
elseif IsPlayVoice("eve019") then
voiceId = 019

--ME cutscene1 (EVC_ST_005) - landing?
elseif IsPlayVoice("eve010") then
voiceId = 010
elseif IsPlayVoice("aln003") then
voiceId = 003
elseif IsPlayVoice("lls015") then
voiceId = 015
elseif IsPlayVoice("eve020") then
voiceId = 020
elseif IsPlayVoice("dnb017") then
voiceId = 017

--ME cutscene2 EVC_ST_006 gau flies over
elseif IsPlayVoice("eve020") then
voiceId = 020
elseif IsPlayVoice("eve021") then
voiceId = 021
end

	--radar red rect
	--(point detection distance, centerX, centerY, width, height, true->square false->circle)
	-- ミッションエリア表示（エリア表示範囲については内部判定済み）
	DrawMissionArea( Rader.Range, Rader.CenterX, Rader.CenterY, Rader.Width, Rader.Height, true )

	
	-- ＨＰ警告処理
	if GetLifeRate() < WARNING_LIFE_RATE then
		SetTrig( "WARNING_LIFE_TRIG" )
	else
		ResetTrig( "WARNING_LIFE_TRIG" )
	end


	-- タイム処理
	if GetIndicateTime() <= WARN_TIME_FRAME then
		SetTrig( "WARN_TIMER_TRIG" )
	end

	-- 毎回の更新処理
	Update()

	-- マーカー表示処理
	if not Check( "REMOVE_MAKER_TRIG" ) then
		DrawMaker()
	end

	-- 削除処理
	if Trigger( "REMOVE_MAKER_TRIG" ) then
		DrawMakerOut()
	end
	
	-- ポイント表示処理
	if not Check( "REMOVE_POINT_TRIG" ) then
		DrawRader()
	end

	-- ポイント削除処理
	if Trigger( "REMOVE_POINT_TRIG" ) then
		DrawRaderOut()
	end

	-- 方位系表示処理
	if not Check( "REMOVE_DIR_POINT_TRIG" ) then
		DrawDir()
	end

	-- 方位系削除処理
	if Trigger( "REMOVE_DIR_POINT_TRIG" ) then
		DrawDirOut()
	end




	-- 武器メイン空状態リリース
	if Release( "WEAPON_SELECT_MAIN_EMPTY_TRIG" ) then
		WeaponSelectMainEmptyOut()
	-- 武器SUB1空状態トリガー
	elseif Release( "WEAPON_SELECT_SUB1_EMPTY_TRIG" ) then
		WeaponSelectSub1EmptyOut()
	-- 武器SUB2空状態トリガー
	elseif Release( "WEAPON_SELECT_SUB2_EMPTY_TRIG" ) then
		WeaponSelectSub2EmptyOut()
	end


	-- 警告、非警告時のメイン処理
	if Check( "WARNING_LIFE_TRIG" ) then
		WarningMain()
	else
		UnwarningMain()
	end



	-- 武器が空状態トリガー
	if Trigger( "WEAPON_SELECT_MAIN_EMPTY_TRIG" ) then
		WeaponSelectMainEmptyIn()
	elseif Trigger( "WEAPON_SELECT_SUB1_EMPTY_TRIG" ) then
		WeaponSelectSub1EmptyIn()
	elseif Trigger( "WEAPON_SELECT_SUB2_EMPTY_TRIG" ) then
		WeaponSelectSub2EmptyIn()
	end


	-- 補給可能エリア時
	if Trigger( "SUPPLY_AREA_TRIG" ) then
		Message( "Supply_TEX_OPEN" )
	end

	-- 補給可能エリア外
	if Release( "SUPPLY_AREA_TRIG" ) then
		Message( "Supply_TEX_OUT" )
	end

	-- スラスターヒート時
	if Trigger( "HEAT_THRUSTOR_TRIG" ) then
		OverHeatIn()
	end

	-- スラスターヒート収まり時
	if Release( "HEAT_THRUSTOR_TRIG" ) then
		Message( "OVERHEAT_TEX_OUT" )
	end

	-- 焼付け時
	if Trigger( "HEAT_GUN_TRIG" ) then
		Message( "Weapon_Panel_B_RED" )
	end

	-- 焼付け収まり時
	if Release( "HEAT_GUN_TRIG" ) then
		Message( "Weapon_Panel_B" )
	end

	-- 格闘可能時
	if Trigger( "CAN_FIGHT_TRIG" ) then
		Message( "Confront_TEX_OPEN" )
	end

	-- 格闘可能距離外に入ったとき
	if Release( "CAN_FIGHT_TRIG" ) then
		Message( "Confront_TEX_OUT" )
	end
	
	-- ライフの警告
	if Trigger( "WARNING_LIFE_TRIG" ) then
		WarningLifeIn()
	end

	-- ライフの警告解除
	if Release( "WARNING_LIFE_TRIG" ) then
		Message( "Main_Gauge_YEL_IN" )
	end


	-- 各ダメージ処理
	if Trigger( "DAMAGE_LEFT_TRIG" ) then
		Message( "Damege_Left" )
	end

	if Trigger( "DAMAGE_BACK_TRIG" ) then
		Message( "Damege_Back" )
	end

	if Trigger( "DAMAGE_RIGHT_TRIG" ) then
		Message( "Damege_Right" )
	end

	if Trigger( "DAMAGE_FRONT_TRIG" ) then
		Message( "Damege_Noise_A" )
	end


	-- 補給開始（非格闘時？）
	if Trigger( "SUPPLY_WAIT_TRIG" ) then
		SupplyWaitIn()
	end

	-- 補給終了
	if Release( "SUPPLY_WAIT_TRIG" ) then
		SupplyWaitOut()
	end



	-- 敵に対してのレティクル中
	if Check( "ON_RETICLE_ENEMY_TRIG" ) then

		-- 敵に対してのレティクル開始
		if Trigger( "ON_RETICLE_ENEMY_TRIG" ) then
			ReticleEnemyIn()
		else
			ReticleEnemy()
		end

	end

	-- 敵に対してのレティクル終了
	if Release( "ON_RETICLE_ENEMY_TRIG" ) then
		ReticleEnemyOut()
	end


	-- 友軍に対してのレティクル中
	if Check( "ON_RETICLE_FRIEND_TRIG" ) then

		-- 友軍に対してのレティクル開始
		if Trigger( "ON_RETICLE_FRIEND_TRIG" ) then
			ReticleFriendIn()
		else
			ReticleFriend()
		end

	end

	-- 友軍に対してのレティクル終了
	if Release( "ON_RETICLE_FRIEND_TRIG" ) then
		ReticleFriendOut()
	end


	-- ターゲットに対してのレティクル中
	if Check( "ON_RETICLE_TARGET_TRIG" ) then

		-- ターゲットに対してのレティクル開始
		if Trigger( "ON_RETICLE_TARGET_TRIG" ) then
			ReticleTargetIn()
		else
			ReticleTarget()
		end
	end

	-- ターゲットに対してのレティクル開放
	if Release( "ON_RETICLE_TARGET_TRIG" ) then
		ReticleTargetOut()
	end

	-- 交換可能武器レティクル開始
	if Trigger( "ON_RETICLE_CHG_WEAPON_TRIG" ) then
		ReticleChangeWeaponIn()
	end

	-- 交換可能武器レティクル終了
	if Release( "ON_RETICLE_CHG_WEAPON_TRIG" ) then
		ReticleChangeWeaponOut()
	end

	-- 交換不可能武器レティクル開始
	if Trigger( "ON_RETICLE_CHG_N_WEAPON_TRIG" ) then
		ReticleNoChangeWeaponIn()
	end

	-- 交換不可能武器レティクル開放
	if Release( "ON_RETICLE_CHG_N_WEAPON_TRIG" ) then
		ReticleNoChangeWeaponOut()
	end




	-- ロックオン不可能距離イン
	if Trigger( "LOCK_ON_OUT_RANGE_TRIG" ) then
		LockOnOutRangeIn()
	end

	-- ロックオン不可能距離アウト
	if Release( "LOCK_ON_OUT_RANGE_TRIG" ) then
		LockOnOutRangeOut()
	end

	-- ロックオン状態イン
	if Trigger( "LOCK_ON_TRIG" ) then
		LockOnIn()
	end

	-- ロックオン状態アウト
	if Release( "LOCK_ON_TRIG" ) then
		LockOnOut()
	end

	-- ロックオン可能距離中
	if Check( "LOCK_ON_IN_RANGE_TRIG" ) then

		-- ロックオン可能距離イン
		if Trigger( "LOCK_ON_IN_RANGE_TRIG" ) then
			LockOnInRangeIn()
		else
			LockOnInRange()
		end
	end

	-- ロックオン可能距離アウト
	if Release( "LOCK_ON_IN_RANGE_TRIG" ) then
		LockOnInRangeOut()
	end
	

	-- 敵ロックオン中
	if Check( "LOCK_ON_ENEMY_TRIG" ) then

		-- 敵ロックオン開始
		if Trigger( "LOCK_ON_ENEMY_TRIG" ) then
			LockOnEnemyIn()
		else
			LockOnEnemy()
		end
	end

	-- 敵ロックオン解除
	if Release( "LOCK_ON_ENEMY_TRIG" ) then
		LockOnEnemyOut()
	end


	-- ターゲットロックオン中
	if Check( "LOCK_ON_TARGET_TRIG" ) then

		-- ターゲットロックオン
		if Trigger( "LOCK_ON_TARGET_TRIG" ) then
			LockOnTargetIn()
		else
			LockOnTarget()
		end
	end

	-- ターゲットロックオン解除
	if Release( "LOCK_ON_TARGET_TRIG" ) then
		LockOnTargetOut()
	end


	-- 鍔迫り合い中（格闘中のみ？）
	if Check( "DEAD_HEAT_TRIG" ) then

		-- 鍔迫り合い開始時
		if Trigger( "DEAD_HEAT_TRIG" ) then
			DeadHeatIn()
		else
			DeadHeat()
		end
	end


	-- 鍔迫り合い終了時
	if Release( "DEAD_HEAT_TRIG" ) then
		DeadHeatOut()
	end


	-- ブレイク表示終了
	if Release( "WEAPON_BREAK_TRIG" ) then
		WeaponBreakOut()
	end


	-- ビームレティクル使用終了
	if Release( "USE_BEAM_RETICLE_TRIG" ) then
		BeamReticleOut()
	end


	-- 実弾レティクル使用終了
	if Release( "USE_BULLET_RETICLE_TRIG" ) then
		BulletReticleOut()

	end


	-- バズーカレティクル使用終了
	if Release( "USE_BAZOOKA_RETICLE_TRIG" ) then
		BazookaReticleOut()
	end


	-- グレネードレティクル使用終了
	if Release( "USE_GRENADE_RETICLE_TRIG" ) then
		GrenadeReticleOut()
	end



	-- マルチロックレティクル使用終了
	if Release( "USE_MULTI_RETICLE_TRIG" ) then
		MultiReticleOut()
	end



	-- EXAM保持チェック
	if Check( "HAS_EXAM_TRIG" ) then
		HasExam()

		-- EXAMシステム使用トリガー
		if Trigger( "HAS_EXAM_TRIG" ) then
			HasExamIn()
		end

		-- EXAMシステム保持キャンセル（無いよ）
		-- if Release( "HAS_EXAM_TRIG" ) then
		-- 	HasExamOut()
		-- end

		-- EXAMシステム使用トリガー
		if Trigger( "USE_EXAM_TRIG" ) then
			isExamWarning = false
			Voice( EXAM_VOICE )
			Sound( SE_EXAM_TIMER_START )
		end

		if Release( "USE_EXAM_TRIG" ) then
			ExamShutdown()
		end

		-- EXAM警告音発生
		if EXAM_WARNING_FRAME > GetExamTime() then

			if not isExamWarning then
				Sound( SE_WARNING_EXAM_TIMER )
				isExamWarning = true
			end
		end


		-- EXAMシステムボイスがなっていないとき
		if IsPlayVoice( EXAM_VOICE ) == false then
			if isExam then
				UseExamOut()
			end
			isExam = false
		end
	
		-- EXAMシステムボイスがなっているとき
		if IsPlayVoice( EXAM_VOICE ) then
			if isExam == false then
				UseExamIn()
			end
			isExam = true
		end
	end

	-- タイマー使用終了トリガー
	if Release( "USE_TIMER_TRIG" ) then
		UseTimerOut()
	end

	-- タイマー使用チェック
	if Check( "USE_TIMER_TRIG" ) then
		
		if Check( "WARN_TIMER_TRIG" ) then
			UseWarnTimer()
		else
			UseTimer()
		end

		if Trigger( "WARN_TIMER_TRIG" ) then
			WarnTimerIn()
		end

		if Release( "WARN_TIMER_TRIG" ) then
			WarnTimerOut()
		end

		if Trigger( "USE_TIMER_TRIG" ) then
			UseTimerIn()
		end

	end




	-- フォーメーション取得
	if Trigger( "SET_FORM_A_N_TRIG" ) then
		FormANIn()
	elseif Trigger( "SET_FORM_A_O_TRIG" ) then
		FormAOIn()
	elseif Trigger( "SET_FORM_A_D_TRIG" ) then
		FormADIn()
	elseif Trigger( "SET_FORM_V_N_TRIG" ) then
		FormVNIn()
	elseif Trigger( "SET_FORM_V_O_TRIG" ) then
		FormVOIn()
	elseif Trigger( "SET_FORM_V_D_TRIG" ) then
		FormVDIn()
	elseif Trigger( "SET_FORM_B_N_TRIG" ) then
		FormBNIn()
	elseif Trigger( "SET_FORM_B_O_TRIG" ) then
		FormBOIn()
	elseif Trigger( "SET_FORM_B_D_TRIG" ) then
		FormBDIn()
	end



end

-- 弾が空でないときの弾設定
function SetNotEmptyBlt()
	mainNotEmpty 	= not Check( "WEAPON_SELECT_MAIN_EMPTY_TRIG" )
	sub1NotEmpty	= not Check( "WEAPON_SELECT_SUB1_EMPTY_TRIG" )
	sub2NotEmpty	= not Check( "WEAPON_SELECT_SUB2_EMPTY_TRIG" )

	if mainNotEmpty and sub1NotEmpty and sub2NotEmpty then
		SetBulletNum(voiceId)
	end
end

-- 格闘状態に入ったとき
function OnFightMode()
	
	-- 弾のタイプ設定（このフレーム中に切り替わった場合の対処）
	AtWeaponType()

	--- 各状態から格闘状態移行
	if Trigger( "WEAPON_MAIN_FIGHT_OPEN_TRIG" ) then
		WeaponMainFightIn()
	elseif Trigger( "WEAPON_SUB1_FIGHT_OPEN_TRIG" ) then
		WeaponSub1FightIn()
	elseif Trigger( "WEAPON_SUB2_FIGHT_OPEN_TRIG" ) then
		WeaponSub2FightIn()
	end

end

-- 武器の状態選択
function AtWeaponType()

	-- 武器メイン非選択時
	if Release( "WEAPON_MAIN_TRIG" ) then
		WeaponMainOut()
	-- 武器SUB1非選択時
	elseif Release( "WEAPON_SUB1_TRIG" ) then
		WeaponSub1Out()
	-- 武器SUB2非選択時
	elseif Release( "WEAPON_SUB2_TRIG" ) then
		WeaponSub2Out()
	end


	-- 武器トリガーに関しては空表示を優先させる
	if Trigger( "WEAPON_MAIN_TRIG" ) then
		WeaponMainIn()
	elseif Trigger( "WEAPON_SUB1_TRIG" ) then
		WeaponSub1In()
	elseif Trigger( "WEAPON_SUB2_TRIG" ) then
		WeaponSub2In()
	end


end

-- 射撃モード中
function AtShootMode()
	-- ビームレティクル使用中（射撃時のみ？）
	if Check( "USE_BEAM_RETICLE_TRIG" ) then
		-- ビームレティクル使用開始（射撃時のみ？）
		if Trigger( "USE_BEAM_RETICLE_TRIG" ) then
			BeamReticleIn()
		else
			BeamReticle()
		end
	-- 実弾レティクル使用中（射撃時のみ？）
	elseif Check( "USE_BULLET_RETICLE_TRIG" ) then

		-- 実弾レティクル使用開始
		if Trigger( "USE_BULLET_RETICLE_TRIG" ) then
			BulletReticleIn()
		else
			BulletReticle()
		end
	-- バズーカレティクル使用中（射撃時のみ？）
	elseif Check( "USE_BAZOOKA_RETICLE_TRIG" ) then

		-- バズーカレティクル使用開始
		if Trigger( "USE_BAZOOKA_RETICLE_TRIG" ) then
			BazookaReticleIn()
		else
			BazookaReticle()
		end
	-- グレネードレティクル使用中（射撃時のみ？）
	elseif Check( "USE_GRENADE_RETICLE_TRIG" ) then

		-- グレネードレティクル使用開始
		if Trigger( "USE_GRENADE_RETICLE_TRIG" ) then
			GrenadeReticleIn()
		end

		-- 銃焼付け開始
		if Trigger( "HEAT_GUN_TRIG" ) then
			Message( "Site_D_Heat" )
		else
			Message( "Site_D_NoHeat" )
		end
		
		GrenadeReticle()
	-- マルチロックレティクル使用中
	elseif Check( "USE_MULTI_RETICLE_TRIG" ) then

		-- マルチロックレティクル使用開始
		if Trigger( "USE_MULTI_RETICLE_TRIG" ) then
			MultiReticleIn()
		else
			MultiReticle()
		end
	end

	-- 空でないときの弾数設定
	SetNotEmptyBlt()

	-- 武器タイプ設定
	AtWeaponType()

end

-- 格闘オフ移行
function OffFightMode()

	if Check( "WEAPON_MAIN_TRIG" ) then
		WeaponMainFightOut()
	elseif Check( "WEAPON_SUB1_TRIG" ) then
		WeaponSub1FightOut()
	elseif Check( "WEAPON_SUB2_TRIG" ) then
		WeaponSub2FightOut()
	end

end

-- 格闘モード中
function AtFightMode()

	-- ブレイク表示イン（格闘中のみ？）
	if Trigger( "WEAPON_BREAK_TRIG" ) then
		WeaponBreakIn()
	end



end



-- 警告時のメイン処理
function WarningMain()

	WarningUpdate()

	-- 通常モード開放
	if Trigger( "NORMAL_MODE_TRIG" ) then
		WarningNormalIn()
		SetBulletNum()
	end


	-- 格闘中
	if Check( "FIGHT_MODE_TRIG" ) then
		FightMode()

		-- 格闘開始
		if Trigger( "FIGHT_MODE_TRIG" ) then
			WarnFightModeIn()
			OnFightMode()
		end
	-- 射撃中
	else
		WarnShootMode()

		-- 射撃開始
		if Trigger( "SHOOT_MODE_TRIG" ) then
			ShootModeIn()
		end
		AtShootMode()
	end


	-- 格闘開放処理
	if Release( "FIGHT_MODE_TRIG" ) then


		WarnFightModeOut()
		OffFightMode()
		SetBulletNum()
	end

	
end

-- 非警告時のメイン処理
function UnwarningMain()

	UnwarningUpdate()

	-- 通常モード開放
	if Trigger( "NORMAL_MODE_TRIG" ) then
		NormalIn()
		SetBulletNum()
	end



	-- 格闘中
	if Check( "FIGHT_MODE_TRIG" ) then
		FightMode()

		-- 格闘開始
		if Trigger( "FIGHT_MODE_TRIG" ) then
			FightModeIn()
			OnFightMode()
		end
	-- 射撃中
	else
		ShootMode()

		-- 射撃開始
		if Trigger( "SHOOT_MODE_TRIG" ) then
			ShootModeIn()
		end
		AtShootMode()
	end

	-- 格闘開放処理
	if Release( "FIGHT_MODE_TRIG" ) then

		FightModeOut()
		OffFightMode()
		SetBulletNum()

	end



end


-- ポイント表示のモデルの表示登録処理
function RegistPoint(points, name, pointIdx, targetIdx)
	dist = GetPointDistinction( name, targetIdx )
	for idx = 1, points.MdlNum do
		if points[idx].Dist then
			if points[idx].Dist == dist then
				return false
			end
		end
	end

	-- 表示対象として登録する
	points[pointIdx].Dist = dist
	return true

end

-- ポイントの消去処理
function ClosePoint( point, outAnim )

	if point.State ~= NONE then
		point.State = NONE
		Anim(point.MdlName, outAnim )
		point.Dist = nil
	end
end

-- ポイントの位置更新
function UpdatePoint( points, name, rader, mdlIdx, pointIdx, myX, myZ, rotY )

	tgX, tgY, tgZ 	= GetPointPos( name, pointIdx )
	dis		= GetPointDistance( name, pointIdx )

	if dis < rader.Range then
		x = tgX - myX
		y = tgZ - myZ
		x = -x
		x = x * rader.Height / (rader.Range * 2)
		y = y * rader.Width  / (rader.Range * 2)

		x, y	= Rotate2D( x, y, 0, 0, rotY )
		x	= x + rader.CenterX
		y	= y + rader.CenterY
		Draw( points[mdlIdx].MdlName, 0.0, 0.0, 0.0, false, x, y)

		if points[mdlIdx].State == NONE then
			Anim(points[mdlIdx].MdlName, points.InAnim )
			points[mdlIdx].State = SHOW
		end

	else
		ClosePoint( points[mdlIdx], points.OutAnim )
	end

end

-- レーダーのポイント位置表示処理
function DrawRaderPoints( rader, points, name )

	rotX, rotY, rotZ 	= GetRotation()
	rotY 			= GetYAxisDir( rotX, rotY, rotZ )
	targetNum 		= GetPointNum( name )
	myX, myY, myZ 		= GetPosition()

	targetIdx		= 0

	for mdlIdx = 1, points.MdlNum do

		-- 既に識別子登録されている場合
		if points[mdlIdx].Dist then

			-- マーカーの識別子を検索（C側の関数）
			pointIdx = FindPointDistinction( name, points[mdlIdx].Dist )
		
			-- ヒットした。
			if pointIdx then
				UpdatePoint( points, name, rader, mdlIdx, pointIdx, myX, myZ, rotY )

			-- 見つからなかった。
			else
				-- 消去処理
				ClosePoint( points[mdlIdx], points.OutAnim )
			end

		-- 未登録の場合
		else

			-- ターゲットがすべて無くなるまで登録を試みる
			for idx = targetIdx, targetNum - 1 do
				-- 登録を試みる
				if RegistPoint(points, name, mdlIdx, idx) then
					-- 既に登録ずみなのでずらす
					targetIdx = idx + 1
					-- 見つかった場合は関数内で登録しているのでループを抜ける
					break
				end

				-- 登録され無いものはずらしていく
				targetIdx = idx + 1
			end
		end

	end

end

-- 方位系ポイントの表示位置更新処理
function UpdateDirPoint( points, name, dir, mdlIdx, pointIdx, myX, myZ, rotY )

	tgX, tgY, tgZ 	= GetPointPos( name, pointIdx )
	dis		= GetPointDistance( name, pointIdx )
	tempX, tempY, tempZ 	= Subtract3D( tgX, tgY, tgZ, myX, 0, myZ)
	tempX, tempY, tempZ 	= Rotate3D( tempX, tempY, tempZ, 0.0, rotY, 0.0)
	tempX			= - tempX
	deg = AtanToDeg(tempZ, tempX)

	if dis < dir.Range and deg > 0.0 then
		deg	= deg - dir.Angle
		x 	= dir.Width / dir.Angle * deg

		temp2dX	= -x
		temp2dY	= dir.CenterY

		Draw( points[mdlIdx].MdlName, 0.0, 0.0, 0.0, false, temp2dX, temp2dY)

		if points[mdlIdx].State == NONE then
			Anim(points[mdlIdx].MdlName, points.InAnim )
			points[mdlIdx].State = SHOW
		end
	else
		ClosePoint( points[mdlIdx], points.OutAnim )
	end

end

-- 方位系のポイントの表示処理
function DrawDirPoints( dir, points, name )

	rotX, rotY, rotZ 	= GetRotation()
	rotY 			= GetYAxisDir( rotX, rotY, rotZ )
	targetNum 		= GetPointNum( name )
	myX, myY, myZ 		= GetPosition()

	targetIdx		= 0

	for mdlIdx = 1, points.MdlNum do

		-- 既に識別子登録されている場合
		if points[mdlIdx].Dist then

			-- マーカーの識別子を検索（C側の関数）
			pointIdx = FindPointDistinction( name, points[mdlIdx].Dist )
		
			-- ヒットした。
			if pointIdx then
				UpdateDirPoint( points, name, dir, mdlIdx, pointIdx, myX, myZ, rotY )

			-- 見つからなかった。
			else
				-- 消去処理
				ClosePoint( points[mdlIdx], points.OutAnim )
			end

		-- 未登録の場合
		else

			-- ターゲットがすべて無くなるまで登録を試みる
			for idx = targetIdx, targetNum - 1 do
				-- 登録を試みる
				if RegistPoint(points, name, mdlIdx, idx) then
					-- 既に登録ずみなのでずらす
					targetIdx = idx + 1
					-- 見つかった場合は関数内で登録しているのでループを抜ける
					break
				end

				-- 登録され無いものはずらしていく
				targetIdx = idx + 1
			end
		end

	end

end




-- マーカーの表示登録処理
function RegistTarget(makers, name, makerIdx, targetIdx)
	dist = GetMakerDistinction( name, targetIdx )
	for idx = 1, makers.MdlNum do
		if makers[idx].Dist == dist then
			return false
		end
	end

	makers[makerIdx].Dist = dist
	return true

end

-- マーカーの表示登録削除処理
function CloseNFTarget( makers, makerIdx )

	if makers[makerIdx].State ~= NONE then
		makers[makerIdx].State = NONE
		Anim(makers[makerIdx].MdlName, makers.OutAnim )
	end
	makers[makerIdx].Dist = nil
end

-- ある距離の範囲のマーカーの更新処理
function UpdateNFTarget( makers, name, near, far, makerIdx, targetIdx)
	distance = GetMakerDistance( name, targetIdx)
	if distance > near and distance < far then
		x, y, z = GetMakerPos( name, targetIdx)
		Draw( makers[makerIdx].MdlName, x, y, z, true )
			
		if IsAnimEnd( makers[makerIdx].MdlName ) == false then
			return true
		end

		if IsReticleMaker( name, targetIdx ) then
			makers[makerIdx].IsReticle = true
		else
			makers[makerIdx].IsReticle = false
		end

		if makers[makerIdx].IsReticle then
			if makers[makerIdx].State ~= ON_RETICLE then
				makers[makerIdx].State = ON_RETICLE
				Anim(makers[makerIdx].MdlName, makers.InReticleAnim )
			end

		else
			if makers[makerIdx].State == NONE then
				makers[makerIdx].State = OFF_RETICLE
				Anim(makers[makerIdx].MdlName, makers.InAnim )
			elseif makers[makerIdx].State == ON_RETICLE then
				makers[makerIdx].State = OFF_RETICLE
				Anim(makers[makerIdx].MdlName, makers.OutReticleAnim )
			elseif makers[makerIdx].State == OFF_RETICLE then
				if makers.ShowAnim then
					frame =  CalcPosParFrame( near, far, distance, makers.ShowFrame )
					Anim(makers[makerIdx].MdlName, makers.ShowAnim, frame, false, 0.0 )
				end
			end
		end
	else
		CloseNFTarget( makers, makerIdx )
	end

end

-- 開始距離と終了距離が決まっているマーカー表示処理
function DrawMakerNF( makers, name, near, far )

	targetNum 		= GetMakerNum( name )
	targetIdx		= 0

	for mdlIdx = 1, makers.MdlNum do

		-- 既に識別子登録されている場合
		if makers[mdlIdx].Dist then

			-- マーカーの識別子を検索（C側の関数）
			makerIdx = FindMakerDistinction( name, makers[mdlIdx].Dist )
		
			-- ヒットした。
			if makerIdx then
				UpdateNFTarget( makers, name, near, far, mdlIdx, makerIdx )

			-- 見つからなかった。
			else
				-- 消去処理
				CloseNFTarget( makers, mdlIdx )
			end

		-- 未登録の場合
		else

			-- ターゲットがすべて無くなるまで登録を試みる
			for idx = targetIdx, targetNum - 1 do
				-- 登録を試みる
				if RegistTarget(makers, name, mdlIdx, idx) then
					-- 既に登録ずみなのでずらす
					targetIdx = idx + 1
					-- 見つかった場合は関数内で登録しているのでループを抜ける
					break
				end

				-- 登録され無いものはずらしていく
				targetIdx = idx + 1
			end
		end

	end


end

-- ある範囲のマーカー登録削除処理
function CloseAreaTarget( makers, makerIdx )
	-- 直接消去で対応してみる。
	if IsAnimEnd( makers[makerIdx].MdlName ) == false then
		return
	end


	if makers[makerIdx].State ~= NONE then
		if makers[makerIdx].State == IN_AREA then
			makers[makerIdx].State = OUT_AREA
			Anim(makers[makerIdx].MdlName, makers.InAreaOutAnim )
		elseif makers[makerIdx].State == IN_AREA_LOOP then
			makers[makerIdx].State = OUT_AREA
			Anim(makers[makerIdx].MdlName, makers.InAreaOutAnim )
		elseif makers[makerIdx].State == R_IN_AREA then
			makers[makerIdx].State = OUT_AREA
			Anim(makers[makerIdx].MdlName, makers.InAreaOutAnim )
		else
			makers[makerIdx].State = NONE
			Anim(makers[makerIdx].MdlName, makers.OutAreaOutAnim )
			makers[makerIdx].Dist = nil
		end
	end
end



-- ある範囲のマーカー登録更新処理
function UpdateAreaTarget( makers, name, area, makerIdx, targetIdx)

	distance = GetMakerDistance( name, targetIdx)
	if distance < makers.InDistance then

		x, y, z = GetMakerPos( name, targetIdx)
		-- 最低限表示はする
		Draw( makers[makerIdx].MdlName, x, y, z, true)

		if IsAnimEnd( makers[makerIdx].MdlName ) == false then
			return true
		end


		isReticle 	= IsReticleMaker( name, targetIdx) 
		isArea		= false
		if distance < area then
			isArea = true
		end
		
		-- アニメーション分岐
		if isReticle and isArea then
			if makers[makerIdx].State == NONE then
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.OutAreaInAnim )
			elseif makers[makerIdx].State == IN_AREA then
				makers[makerIdx].State = R_IN_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaReticleAnim )
			elseif makers[makerIdx].State == IN_AREA_LOOP then
				makers[makerIdx].State = R_IN_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaReticleAnim )
			elseif makers[makerIdx].State == R_IN_AREA then
				-- 安定
			else
				makers[makerIdx].State = IN_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaInAnim )
			end
		elseif isReticle == false and isArea then
			if makers[makerIdx].State == NONE then
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.OutAreaInAnim )
			elseif makers[makerIdx].State == OUT_AREA then
				makers[makerIdx].State = IN_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaInAnim )
			elseif makers[makerIdx].State == IN_AREA then
				makers[makerIdx].State = IN_AREA_LOOP
				Anim(makers[makerIdx].MdlName, makers.InAreaLoopAnim )
			elseif makers[makerIdx].State == IN_AREA_LOOP then
				-- 安定
			else
				makers[makerIdx].State = IN_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaInAnim )				
			end
		elseif isReticle and isArea == false then
			if makers[makerIdx].State == NONE then
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.OutAreaInAnim )
			elseif makers[makerIdx].State == OUT_AREA then
				makers[makerIdx].State = R_OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.OutAreaReticleAnim )
			elseif makers[makerIdx].State == R_OUT_AREA then
				-- 安定
			else
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaOutAnim )
			end
		else
			if makers[makerIdx].State == NONE then
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.OutAreaInAnim )
			elseif makers[makerIdx].State == R_OUT_AREA then
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.OutAreaInAnim )
			elseif makers[makerIdx].State == OUT_AREA then
				-- 安定
			else
				makers[makerIdx].State = OUT_AREA
				Anim(makers[makerIdx].MdlName, makers.InAreaOutAnim )
			end
		end

		return true
	else
		-- 表示をしなくする
		CloseAreaTarget( makers, makerIdx )
		return false
	end

	return false
end


-- 一定範囲でのマーカー表示関数
function DrawAreaMaker( makers, name, area)

	targetNum 		= GetMakerNum( name )
	targetIdx		= 0

	-- マーカーの作成数分ループする。
	for mdlIdx = 1, makers.MdlNum do

		-- 既に識別子登録されている場合
		if makers[mdlIdx].Dist then

			-- マーカーの識別子を検索（C側の関数）
			makerIdx = FindMakerDistinction( name, makers[mdlIdx].Dist )
		
			-- ヒットした。
			if makerIdx then
				UpdateAreaTarget( makers, name, area, mdlIdx, makerIdx )

			-- 見つからなかった。
			else
				-- 消去処理
				CloseAreaTarget( makers, mdlIdx )
			end

		-- 未登録の場合
		else

			-- ターゲットがすべて無くなるまで登録を試みる
			for idx = targetIdx, targetNum - 1 do
				-- 登録を試みる
				if RegistTarget(makers, name, mdlIdx, idx) then
					-- 既に登録ずみなのでずらす
					targetIdx = idx + 1
					-- 見つかった場合は関数内で登録しているのでループを抜ける
					break
				end

				-- 登録され無いものはずらしていく
				targetIdx = idx + 1
			end

		end

	end

end


-- ポイントの強制消去処理
function RemovePoints( points )
	for mdlIdx = 1, points.MdlNum do
		points[mdlIdx].State = NONE
		Anim(points[mdlIdx].MdlName, points.OutAnim, 50.0 )
		points[mdlIdx].Dist = nil
	end
end

-- 範囲マーカーの強制消去処理
function RemoveNFTarget( makers )
	for mdlIdx = 1, makers.MdlNum do
		makers[mdlIdx].State = NONE
		Anim(makers[mdlIdx].MdlName, makers.OutAnim, 50.0 )
		makers[mdlIdx].Dist = nil
	end
end

-- まーかーの強制消去処理
function RemoveAreaTarget( makers )
	for mdlIdx = 1, makers.MdlNum do
		makers[mdlIdx].State = NONE
		Anim(makers[mdlIdx].MdlName, makers.OutAreaOutAnim, 50.0 )
		makers[mdlIdx].Dist = nil
	end
end
