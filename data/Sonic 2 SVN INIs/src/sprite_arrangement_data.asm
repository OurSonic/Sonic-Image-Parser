/*
	Sprite Arrangements -
		Two words at each position represent the vertical and horizontal
		offsets, relative to the object's X/Y location in the level, at
		which to draw each sprite.
		
		e.g. An object composed of 3x2 sprites might be arranged like 
		this (see data at Bank 31, $9dbE which is used for sonic's 
		sprites):
		
			 -12   -4   +4
			.----.----.----.
		-32	|    |    |    |
			| 1  | 2  | 3  |
			|    |    |    |
			|----+----+----|
		-16	|    |    |    |
			| 4  | 5  | 6  |
			|    |    |    |
		0	'----'----'----'
		           0
		
		You can see from the diagram that the sprite's origin is
		bottom-centre.

*/

SprArrange_1x1_BC:		;$9D2E
.dw -16, -4

;	 _ _
;	| | |
;   |_|_|
;
SprArrange_2x1_BC:		;$9D32
DATA_B31_9D32:
.dw	-16, -8
.dw -16, 0


;	 _ 
;	|1|
;   |_|
;	|2|
;	|_|
SprArrange_1x2_BC:
.dw -32, -4
.dw -16, -4


;	 _
;	|1|_
;   |_|2|
;	  |_|
;
SprArrange_2x1_Diag:
.dw -24, -8
.dw -16, 0


;	 _ _ _
;	|1|2|3|
;	|_|_|_|
;
SprArrange_3x1:		;$9D4A
.dw -16, -12
.dw -16, -4
.dw -16, 4


;
;	 _
;	|1|
;	|_|
;	|2|
;	|_|
;	|3|
;	|_|
;
SprArrange_1x1_1x1_1x1:	;$9D56
.dw -48, -4
.dw -32, -4
.dw -16, -4


;	 _ _
;	|1|2|
;	|_|_|
;	|3|4|
;	|_|_|
;
SprArrange_2x2_BC:		;$9D62
.dw -32, -8
.dw -32, 0
.dw -16, -8
.dw -16, 0


;	 _ _ _ _
;	|1|2|3|4|
;	|_|_|_|_|
;
SprArrange_4x1:		;$9D72
.dw -16, -16
.dw -16, -8
.dw -16, 0
.dw -16, 8


;	 _ _ _
;	|1|2|3|
;	|_|_|_|
;	 |4|5|
;	 |_|_|
;
SprArrange_3x1_2x1:		;$9D82
.dw -32, -12
.dw -32, -4
.dw -32, 4
.dw -16, -8
.dw -16, 0


;	   _ _
;	  |1|2|
;	 _|_|_|
;	|3|4|5|
;	|_|_|_|
;
SprArrange_2x1_3x1:		;$9D96
.dw -32, -4
.dw -32, 4
.dw -16, -12
.dw -16, -4
.dw -16, 4


;	 _ _ _ _ _
;	|1|2|3|4|5|
;	|_|_|_|_|_|
;
SprArrange_5x1:		;$9DAA
.dw -16, -20
.dw -16, -12
.dw -16, -4
.dw -16, 4
.dw -16, 12

;	 _ _ _
;	|1|2|3|
;	|_|_|_|
;	|4|5|6|
;	|_|_|_|
;
SprArrange_3x2_BC:		;$9DBE
.dw -32, -12
.dw -32, -4
.dw -32, 4
.dw -16, -12
.dw -16, -4
.dw -16, 4


;	 _ _
;	|1|2|
;	|_|_|
;	|3|4|
;	|_|_|
;	|5|6|
;	|_|_|
;
SprArrange_2x3:		;$9DD6
.dw -48, -8
.dw -48, 0
.dw -32, -8
.dw -32, 0
.dw -16, -8
.dw -16, 0


;	 _ _ _ _
;	|1|2|3|4|
;	|_|_|_|_|
;	|5|6|
;	|_|_|
;	|7|8|
;	|_|_|		used for SHZ boss
;	|9|A|
;	|_|_|
;
SprArrange_4x1_2x3:		;$9DEE
.dw -64, -16
.dw -64, -8
.dw -64, 0
.dw -64, 8
.dw -48, -16
.dw -48, -8
.dw -32, -16
.dw -32, -8
.dw -16, -16
.dw -16, -8

;	   _ _ _
;	  |1|2|3|
;	 _|_|_|_|
;	|4|5|6|
;	|_|_|_|
SprArrange_3x1_3x1:
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -16, -16
.dw -16, -8
.dw -16, 0


;	 _ _ _ _
;	|1|2|3|4|
;	|_|_|_|_|
;	|5|6|7|
;	|_|_|_|
;	|8|9|
;	|_|_|
;	|A|B|
;	|_|_|
;
SprArrange_4x1_1x3_2x2:		;$9E2E - NOTE: Unused
.dw -64, -16
.dw -64, -8
.dw -64, 0
.dw -64, 8
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -32, -16
.dw -32, -8
.dw -16, -16
.dw -16, -8


;	   _ _
;	  |1|2|
;	  |_|_|
;	  |3|4|
;	 _|_|_|
;	|5|6|7|
;	|_|_|_|
;
SprArrange_2x2_1x3:		;$9E5A
.dw	-48, -4
.dw -48, 4
.dw -32, -4
.dw -32, 4
.dw -16, -12
.dw -16, -4
.dw -16, 4


;	 _ _ _ _
;	|1|2|3|4|
;	|_|_|_|_|
;	|5|6|7|8|
;	|_|_|_|_|
;
SprArrange_4x2:		;$9E76
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -16, -16
.dw -16, -8
.dw -16, 0
.dw -16, 8


;	 _ _
;	|1|2|
;	|_|_|
;	|3|4|
;	|_|_|
;	|5|6|
;	|_|_|
;	|7|8|
;	|_|_|
;
SprArrange_2x4:		;$9E96
.dw -64, -8
.dw -64, 0
.dw -48, -8
.dw -48, 0
.dw -32, -8
.dw -32, 0
.dw -16, -8
.dw -16, 0


;	 _ _ _
;	|1|2|3|
;	|_|_|_|
;	|4|5|6|
;	|_|_|_|
;	|7|8|9|
;	|_|_|_|
;
SprArrange_3x3:		;$9EB6
.dw -48, -12
.dw -48, -4
.dw -48, 4
.dw -32, -12
.dw -32, -4
.dw -32, 4
.dw -16, -12
.dw -16, -4
.dw -16, 4


;	 _ _ _ _ _
;	|1|2|3|4|5|
;	|_|_|_|_|_|
;	|6|7|8|9|A|
;	|_|_|_|_|_|
;
SprArrange_5x2:		;$9EDA
.dw -32, -20
.dw -32, -12
.dw -32, -4
.dw -32, 4
.dw -32, 12
.dw -16, -20
.dw -16, -12
.dw -16, -4
.dw -16, 4
.dw -16, 12


;	 _     _
;	|1|   |2|
;	|_|_ _|_|
;	|3|4|5|6|
;	|_|_|_|_|
;	  |7|8|
;	  |_|_|			UGZ pincers
;	  |9|A|
;	  |_|_|
;
SprArrange_1x1_1x1_4x1_2x2:		;$9F02
.dw -64, -16
.dw -64, 8
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -32, -8
.dw -32, 0
.dw -16, -8
.dw -16, 0


;	 _ _ _ _ _ _
;	|1|2|3|4|5|6|
;	|_|_|_|_|_|_|
;	    |7|8|
;	    |_|_|		;UGZ pincers
;	    |9|A|
;	    |_|_|
;
SprArrange_6x1_2x2:		;$9F2A
.dw -48, -24
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -48, 16
.dw -32, -8
.dw -32, 0
.dw -16, -8
.dw -16, 0


;	 _ _ _ _
;	|1|2|3|4|
;	|_|_|_|_|
;	|5|6|7|8|
;	|_|_|_|_|
;	  |9|A|B|
;	  |_|_|_|
;
SprArrange_4x2_3x1:		;$9F52
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -16, -8
.dw -16, 0
.dw -16, 8


;	     _
;	    |1|
;	 _ _|_|_ _
;	|2|3|4|5|6|
;	|_|_|_|_|_|
;	|7|8|9|A|B|
;	|_|_|_|_|_|
;
SprArrange_1x1_5x2:		;$9F7E
.dw -48, -4
.dw -32, -20
.dw -32, -12
.dw -32, -4
.dw -32, 4
.dw -32, 12
.dw -16, -20
.dw -16, -12
.dw -16, -4
.dw -16, 4
.dw -16, 12


;	 _ _ _ _
;	|1|2|3|4|
;	|_|_|_|_|
;	|5|6|7|8|
;	|_|_|_|_|
;	|9|A|B|C|
;	|_|_|_|_|
;
SprArrange_4x3:		;$9FAA
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -16, -16
.dw -16, -8
.dw -16, 0
.dw -16, 8


;	 _ _ _ _ _ _
;	|1|2|3|4|5|6|
;	|_|_|_|_|_|_|
;	|7|8|9|A|B|C|
;	|_|_|_|_|_|_|
;
SprArrange_6x2:		;$9FDA
.dw -32, -24
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -32, 16
.dw -16, -24
.dw -16, -16
.dw -16, -8
.dw -16, 0
.dw -16, 8
.dw -16, 16


;	 _ _
;	|1|2|---.
;	|_|_|3|4|---.
;	|7|8|-+-|5|6|
;	|_|_|9|A|-+-|
;		'---|B|C|
;			'---'
SprArrange_2x2_2x2_2x2_Diag:		;$A00A
.dw -34, -26
.dw -34, -18
.dw -32, -8
.dw -32, 0
.dw -30, 12
.dw -30, 20
.dw -18, -26
.dw -18, -18
.dw -16, -8
.dw -16, 0
.dw -14, 12
.dw -14, 20


;	    __ __ __ __ __ __
;	   | 1| 2| 3| 4| 5| 6|
;	 __|__|__|__|__|__|__|__
;	| 7| 8| 9| A| B| C| D| E|
;	|__|__|__|__|__|__|__|__|
;	| F|                 |10|
;	|__|                 |__|
;
SprArrange_6x1_8x1_1x1_1x1:		;$A03A
.dw -48, -24
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -48, 16
.dw -32, -32
.dw -32, -24
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -32, 16
.dw -32, 24
.dw -16, -24
.dw -16, 16


;	       __
;	      | 1|
;	 __ __|__|__ __
;	| 2| 3| 4| 5| 6|
;	|__|__|__|__|__|
;	| 7| 8| 9| A| B|
;	|__|__|__|__|__|
;	| C| D| E| F|10|
;	|__|__|__|__|__|
;
SprArrange_1x1_5x3:		;$A07A
.dw -64, -4
.dw -48, -20
.dw -48, -12
.dw -48, -4
.dw -48, 4
.dw -48, 12
.dw -32, -20
.dw -32, -12
.dw -32, -4
.dw -32, 4
.dw -32, 12
.dw -16, -20
.dw -16, -12
.dw -16, -4
.dw -16, 4
.dw -16, 12


;	 __ __ __ __
;	| 1| 2| 3| 4|
;	|__|__|__|__|
;	| 5| 6| 7| 8|
;	|__|__|__|__|
;	| 9| A| B| C|
;	|__|__|__|__|
;	| D| E| F|10|
;	|__|__|__|__|
;
SprArrange_4x4		;$A0BA
.dw -64, -16
.dw -64, -8
.dw -64, 0
.dw -64, 8
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -16, -16
.dw -16, -8
.dw -16, 0
.dw -16, 8


;	    __ __ __ __ __ __
;	   | 1| 2| 3| 4| 5| 6|
;	 __|__|__|__|__|__|__|__
;	| 7| 8| 9| A| B| C| D| E|
;	|__|__|__|__|__|__|__|__|
;	   | F|10|11|12|13|14|
;	   |__|__|__|__|__|__|
;
SprArrange_1x6_1x8_1x6:		;$A0FA
.dw -48, -24
.dw -48, -16
.dw -48, -8
.dw -48, 0
.dw -48, 8
.dw -48, 16
.dw -32, -32
.dw -32, -24
.dw -32, -16
.dw -32, -8
.dw -32, 0
.dw -32, 8
.dw -32, 16
.dw -32, 24
.dw -16, -24
.dw -16, -16
.dw -16, -8
.dw -16, 0
.dw -16, 8
.dw -16, 16


DATA_B31_A14A:
.db $B0, $FF, $FC, $FF	; $418-$41F

.db $C0, $FF, $EC, $FF
.db $C0, $FF, $F4, $FF	; $420-$427
.db $C0, $FF, $FC, $FF
.db $C0, $FF, $04, $00	; $428-$42F
.db $C0, $FF, $0C, $00

.db $D0, $FF, $EC, $FF	; $430-$437
.db $D0, $FF, $F4, $FF
.db $D0, $FF, $FC, $FF	; $438-$43F
.db $D0, $FF, $04, $00
.db $D0, $FF, $0C, $00	; $440-$447

.db $E0, $FF, $EC, $FF
.db $E0, $FF, $F4, $FF	; $448-$44F
.db $E0, $FF, $FC, $FF
.db $E0, $FF, $04, $00	; $450-$457
.db $E0, $FF, $0C, $00

.db $F0, $FF, $EC, $FF	; $458-$45F
.db $F0, $FF, $F4, $FF
.db $F0, $FF, $04, $00	; $460-$467
.db $F0, $FF, $0C, $00
.db $F0, $FF, $FC, $FF	; $468-$46F
.db $F0, $FF, $00, $00
.db $F0, $FF, $F8, $FF	; $470-$477

.db $E0, $FF, $FC, $FF, $F0, $FF, $FC, $FF	; $478-$47F
.db $E8, $FF, $00, $00, $F0, $FF, $F8, $FF	; $480-$487
.db $F0, $FF, $04, $00, $F0, $FF, $FC, $FF	; $488-$48F
.db $F0, $FF, $F4, $FF, $D0, $FF, $FC, $FF	; $490-$497
.db $E0, $FF, $FC, $FF, $F0, $FF, $FC, $FF	; $498-$49F
.db $E0, $FF, $00, $00, $E0, $FF, $F8, $FF	; $4A0-$4A7
.db $F0, $FF, $00, $00, $F0, $FF, $F8, $FF	; $4A8-$4AF
.db $F0, $FF, $08, $00, $F0, $FF, $00, $00	; $4B0-$4B7
.db $F0, $FF, $F8, $FF, $F0, $FF, $F0, $FF	; $4B8-$4BF
.db $E0, $FF, $04, $00, $E0, $FF, $FC, $FF	; $4C0-$4C7
.db $E0, $FF, $F4, $FF, $F0, $FF, $00, $00	; $4C8-$4CF
.db $F0, $FF, $F8, $FF, $E0, $FF, $FC, $FF	; $4D0-$4D7
.db $E0, $FF, $F4, $FF, $F0, $FF, $04, $00	; $4D8-$4DF
.db $F0, $FF, $FC, $FF, $F0, $FF, $F4, $FF	; $4E0-$4E7
.db $F0, $FF, $0C, $00, $F0, $FF, $04, $00	; $4E8-$4EF
.db $F0, $FF, $FC, $FF, $F0, $FF, $F4, $FF	; $4F0-$4F7
.db $F0, $FF, $EC, $FF, $E0, $FF, $04, $00	; $4F8-$4FF
.db $E0, $FF, $FC, $FF, $E0, $FF, $F4, $FF	; $500-$507
.db $F0, $FF, $04, $00, $F0, $FF, $FC, $FF	; $508-$50F
.db $F0, $FF, $F4, $FF, $D0, $FF, $00, $00	; $510-$517
.db $D0, $FF, $F8, $FF, $E0, $FF, $00, $00	; $518-$51F
.db $E0, $FF, $F8, $FF, $F0, $FF, $00, $00	; $520-$527
.db $F0, $FF, $F8, $FF, $C0, $FF, $08, $00	; $528-$52F
.db $C0, $FF, $00, $00, $C0, $FF, $F8, $FF	; $530-$537
.db $C0, $FF, $F0, $FF, $D0, $FF, $08, $00	; $538-$53F
.db $D0, $FF, $00, $00, $E0, $FF, $08, $00	; $540-$547
.db $E0, $FF, $00, $00, $F0, $FF, $08, $00	; $548-$54F
.db $F0, $FF, $00, $00, $E0, $FF, $00, $00	; $550-$557
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $558-$55F
.db $F0, $FF, $08, $00, $F0, $FF, $00, $00	; $560-$567
.db $F0, $FF, $F8, $FF, $C0, $FF, $08, $00	; $568-$56F
.db $C0, $FF, $00, $00, $C0, $FF, $F8, $FF	; $570-$577
.db $C0, $FF, $F0, $FF, $D0, $FF, $08, $00	; $578-$57F
.db $D0, $FF, $00, $00, $D0, $FF, $F8, $FF	; $580-$587
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $588-$58F
.db $F0, $FF, $08, $00, $F0, $FF, $00, $00	; $590-$597
.db $D0, $FF, $FC, $FF, $D0, $FF, $F4, $FF	; $598-$59F
.db $E0, $FF, $FC, $FF, $E0, $FF, $F4, $FF	; $5A0-$5A7
.db $F0, $FF, $04, $00, $F0, $FF, $FC, $FF	; $5A8-$5AF
.db $F0, $FF, $F4, $FF, $E0, $FF, $08, $00	; $5B0-$5B7
.db $E0, $FF, $00, $00, $E0, $FF, $F8, $FF	; $5B8-$5BF
.db $E0, $FF, $F0, $FF, $F0, $FF, $08, $00	; $5C0-$5C7
.db $F0, $FF, $00, $00, $F0, $FF, $F8, $FF	; $5C8-$5CF
.db $F0, $FF, $F0, $FF, $C0, $FF, $00, $00	; $5D0-$5D7
.db $C0, $FF, $F8, $FF, $D0, $FF, $00, $00	; $5D8-$5DF
.db $D0, $FF, $F8, $FF, $E0, $FF, $00, $00	; $5E0-$5E7
.db $E0, $FF, $F8, $FF, $F0, $FF, $00, $00	; $5E8-$5EF
.db $F0, $FF, $F8, $FF, $D0, $FF, $04, $00	; $5F0-$5F7
.db $D0, $FF, $FC, $FF, $D0, $FF, $F4, $FF	; $5F8-$5FF
.db $E0, $FF, $04, $00, $E0, $FF, $FC, $FF	; $600-$607
.db $E0, $FF, $F4, $FF, $F0, $FF, $04, $00	; $608-$60F
.db $F0, $FF, $FC, $FF, $F0, $FF, $F4, $FF	; $610-$617
.db $E0, $FF, $0C, $00, $E0, $FF, $04, $00	; $618-$61F
.db $E0, $FF, $FC, $FF, $E0, $FF, $F4, $FF	; $620-$627
.db $E0, $FF, $EC, $FF, $F0, $FF, $0C, $00	; $628-$62F
.db $F0, $FF, $04, $00, $F0, $FF, $FC, $FF	; $630-$637
.db $F0, $FF, $F4, $FF, $F0, $FF, $EC, $FF	; $638-$63F
.db $C0, $FF, $08, $00, $C0, $FF, $F0, $FF	; $640-$647
.db $D0, $FF, $08, $00, $D0, $FF, $00, $00	; $648-$64F
.db $D0, $FF, $F8, $FF, $D0, $FF, $F0, $FF	; $650-$657
.db $E0, $FF, $00, $00, $E0, $FF, $F8, $FF	; $658-$65F
.db $F0, $FF, $00, $00, $F0, $FF, $F8, $FF	; $660-$667
.db $D0, $FF, $10, $00, $D0, $FF, $08, $00	; $668-$66F
.db $D0, $FF, $00, $00, $D0, $FF, $F8, $FF	; $670-$677
.db $D0, $FF, $F0, $FF, $D0, $FF, $E8, $FF	; $678-$67F
.db $E0, $FF, $00, $00, $E0, $FF, $F8, $FF	; $680-$687
.db $F0, $FF, $00, $00, $F0, $FF, $F8, $FF	; $688-$68F
.db $D0, $FF, $08, $00, $D0, $FF, $00, $00	; $690-$697
.db $D0, $FF, $F8, $FF, $D0, $FF, $F0, $FF	; $698-$69F
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $6A0-$6A7
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $6A8-$6AF
.db $F0, $FF, $00, $00, $F0, $FF, $F8, $FF	; $6B0-$6B7
.db $F0, $FF, $F0, $FF, $D0, $FF, $FC, $FF	; $6B8-$6BF
.db $E0, $FF, $0C, $00, $E0, $FF, $04, $00	; $6C0-$6C7
.db $E0, $FF, $FC, $FF, $E0, $FF, $F4, $FF	; $6C8-$6CF
.db $E0, $FF, $EC, $FF, $F0, $FF, $0C, $00	; $6D0-$6D7
.db $F0, $FF, $04, $00, $F0, $FF, $FC, $FF	; $6D8-$6DF
.db $F0, $FF, $F4, $FF, $F0, $FF, $EC, $FF	; $6E0-$6E7
.db $D0, $FF, $08, $00, $D0, $FF, $00, $00	; $6E8-$6EF
.db $D0, $FF, $F8, $FF, $D0, $FF, $F0, $FF	; $6F0-$6F7
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $6F8-$6FF
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $700-$707
.db $F0, $FF, $08, $00, $F0, $FF, $00, $00	; $708-$70F
.db $F0, $FF, $F8, $FF, $F0, $FF, $F0, $FF	; $710-$717
.db $E0, $FF, $E8, $FF, $E0, $FF, $10, $00	; $718-$71F
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $720-$727
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $728-$72F
.db $F0, $FF, $E8, $FF, $F0, $FF, $10, $00	; $730-$737
.db $F0, $FF, $08, $00, $F0, $FF, $00, $00	; $738-$73F
.db $F0, $FF, $F8, $FF, $F0, $FF, $F0, $FF	; $740-$747
.db $E0, $FF, $E0, $FF, $E0, $FF, $E8, $FF	; $748-$74F
.db $E0, $FF, $F8, $FF, $E0, $FF, $00, $00	; $750-$757
.db $E0, $FF, $10, $00, $E0, $FF, $18, $00	; $758-$75F
.db $F0, $FF, $E0, $FF, $F0, $FF, $E8, $FF	; $760-$767
.db $F0, $FF, $F8, $FF, $F0, $FF, $00, $00	; $768-$76F
.db $F0, $FF, $10, $00, $F0, $FF, $18, $00	; $770-$777
.db $D0, $FF, $10, $00, $D0, $FF, $08, $00	; $778-$77F
.db $D0, $FF, $00, $00, $D0, $FF, $F8, $FF	; $780-$787
.db $D0, $FF, $F0, $FF, $D0, $FF, $E8, $FF	; $788-$78F
.db $E0, $FF, $18, $00, $E0, $FF, $10, $00	; $790-$797
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $798-$79F
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $7A0-$7A7
.db $E0, $FF, $E8, $FF, $E0, $FF, $E0, $FF	; $7A8-$7AF
.db $F0, $FF, $10, $00, $F0, $FF, $E8, $FF	; $7B0-$7B7
.db $C0, $FF, $FC, $FF, $D0, $FF, $0C, $00	; $7B8-$7BF
.db $D0, $FF, $04, $00, $D0, $FF, $FC, $FF	; $7C0-$7C7
.db $D0, $FF, $F4, $FF, $D0, $FF, $EC, $FF	; $7C8-$7CF
.db $E0, $FF, $0C, $00, $E0, $FF, $04, $00	; $7D0-$7D7
.db $E0, $FF, $FC, $FF, $E0, $FF, $F4, $FF	; $7D8-$7DF
.db $E0, $FF, $EC, $FF, $F0, $FF, $0C, $00	; $7E0-$7E7
.db $F0, $FF, $04, $00, $F0, $FF, $FC, $FF	; $7E8-$7EF
.db $F0, $FF, $F4, $FF, $F0, $FF, $EC, $FF	; $7F0-$7F7
.db $C0, $FF, $08, $00, $C0, $FF, $00, $00	; $7F8-$7FF
.db $C0, $FF, $F8, $FF, $C0, $FF, $F0, $FF	; $800-$807
.db $D0, $FF, $08, $00, $D0, $FF, $00, $00	; $808-$80F
.db $D0, $FF, $F8, $FF, $D0, $FF, $F0, $FF	; $810-$817
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $818-$81F
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $820-$827
.db $F0, $FF, $08, $00, $F0, $FF, $00, $00	; $828-$82F
.db $F0, $FF, $F8, $FF, $F0, $FF, $F0, $FF	; $830-$837
.db $D0, $FF, $10, $00, $D0, $FF, $08, $00	; $838-$83F
.db $D0, $FF, $00, $00, $D0, $FF, $F8, $FF	; $840-$847
.db $D0, $FF, $F0, $FF, $D0, $FF, $E8, $FF	; $848-$84F
.db $E0, $FF, $18, $00, $E0, $FF, $10, $00	; $850-$857
.db $E0, $FF, $08, $00, $E0, $FF, $00, $00	; $858-$85F
.db $E0, $FF, $F8, $FF, $E0, $FF, $F0, $FF	; $860-$867
.db $E0, $FF, $E8, $FF, $E0, $FF, $E0, $FF	; $868-$86F
.db $F0, $FF, $10, $00, $F0, $FF, $08, $00	; $870-$877
.db $F0, $FF, $00, $00, $F0, $FF, $F8, $FF	; $878-$87F
.db $F0, $FF, $F0, $FF, $F0, $FF, $E8, $FF	; $880-$887
.db $B0, $FF, $FC, $FF, $C0, $FF, $0C, $00	; $888-$88F
.db $C0, $FF, $04, $00, $C0, $FF, $FC, $FF	; $890-$897
.db $C0, $FF, $F4, $FF, $C0, $FF, $EC, $FF	; $898-$89F
.db $D0, $FF, $0C, $00, $D0, $FF, $04, $00	; $8A0-$8A7
.db $D0, $FF, $FC, $FF, $D0, $FF, $F4, $FF	; $8A8-$8AF
.db $D0, $FF, $EC, $FF, $E0, $FF, $0C, $00	; $8B0-$8B7
.db $E0, $FF, $04, $00, $E0, $FF, $FC, $FF	; $8B8-$8BF
.db $E0, $FF, $F4, $FF, $E0, $FF, $EC, $FF	; $8C0-$8C7
.db $F0, $FF, $0C, $00, $F0, $FF, $04, $00	; $8C8-$8CF
.db $F0, $FF, $F4, $FF, $F0, $FF, $EC, $FF	; $8D0-$8D7
DATA_B31_A606:
.db $00, $00, $00, $00, $00, $00, $08, $00	; $8D8-$8DF
.db $00, $00, $10, $00, $00, $00, $18, $00	; $8E0-$8E7
.db $00, $00, $20, $00, $00, $00, $28, $00	; $8E8-$8EF
.db $00, $00, $30, $00, $10, $00, $00, $00	; $8F0-$8F7
.db $10, $00, $08, $00, $10, $00, $10, $00	; $8F8-$8FF
.db $10, $00, $18, $00, $10, $00, $20, $00	; $900-$907
.db $10, $00, $28, $00, $10, $00, $30, $00	; $908-$90F
.db $20, $00, $00, $00, $20, $00, $08, $00	; $910-$917
.db $20, $00, $10, $00, $20, $00, $18, $00	; $918-$91F
.db $20, $00, $20, $00, $20, $00, $28, $00	; $920-$927
.db $20, $00, $30, $00, $30, $00, $00, $00	; $928-$92F
.db $30, $00, $08, $00, $30, $00, $10, $00	; $930-$937
.db $30, $00, $18, $00, $30, $00, $20, $00	; $938-$93F
.db $30, $00, $28, $00, $30, $00, $30, $00	; $940-$947
DATA_B31_A676:
.db $D0, $FF, $F4, $FF, $D0, $FF, $FC, $FF	; $948-$94F
.db $D0, $FF, $04, $00, $E0, $FF, $F4, $FF	; $950-$957
.db $E0, $FF, $FC, $FF, $E0, $FF, $04, $00	; $958-$95F
.db $F0, $FF, $F8, $FF, $F0, $FF, $00, $00	; $960-$967
.db $F0, $FF, $04, $00, $F0, $FF, $F4, $FF	; $968-$96F
.db $F6, $FF, $08, $00, $F6, $FF, $F0, $FF	; $970-$977
