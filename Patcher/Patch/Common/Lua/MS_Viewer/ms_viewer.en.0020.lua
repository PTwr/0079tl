--filters out non-playable units to not screw up gallery mech listing
x = {}
n = 1
for i=1,#D_UnitName,1 do
	v=D_UnitName[i]
	if (v.stream_name ~= nil) then
		x[n] = v
		n = n+1
	end
end
D_UnitName=x