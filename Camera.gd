extends Camera

var tile_size = 1


			
onready var tween = $Tween

func _input(event):

	var forward = transform.basis.z.normalized();

	var move_inputs = {
		"ui_right": -forward.cross(Vector3.UP),
		"ui_left": forward.cross(Vector3.UP),
		"ui_up": -forward,
		"ui_down": forward
	}

	var rotate_inputs = {
		"ui_rotate_left": Vector3(0, 90, 0),
		"ui_rotate_right": Vector3(0, -90, 0)
	}

	for key in move_inputs.keys():
		if event.is_action_pressed(key):
			move_player(move_inputs[key])
	
	
	for key in rotate_inputs.keys():
		if event.is_action_pressed(key):
			rotate_player(rotate_inputs[key])
			
	if event.is_action_pressed('mouse_right_click'):
		var pos = get_viewport().get_mouse_position()
		var start = project_ray_origin(pos)
		var end = project_ray_normal(pos) * 100;
		drawline(start, end);
		print(end)
	
func drawline(a, b):
	$Drawer.clear()
	$Drawer.begin(Mesh.PRIMITIVE_LINES)
	$Drawer.set_color(Color(255,1,1))
	$Drawer.add_vertex(a)
	$Drawer.add_vertex(b)
	$Drawer.end()

func move_player(dir):
	if tween.is_active():
		return
	move_tween(dir)

func rotate_player(dir):
	if tween.is_active():
		return
	tween.interpolate_property(
		self, 
		"rotation_degrees",
		self.rotation_degrees, 
		self.rotation_degrees + dir,
		0.2, 
		Tween.TRANS_SINE, 
		Tween.EASE_IN_OUT
	)
	tween.start()

func move_tween(dir):

	tween.interpolate_property(
		self, 
		"translation",
		self.translation, 
		self.translation + dir.normalized() * 4,
		0.2, 
		Tween.TRANS_SINE, 
		Tween.EASE_IN_OUT
	)
	tween.start()

func _process(delta):
	pass
