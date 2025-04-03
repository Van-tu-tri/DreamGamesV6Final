# DreamGamesV5
 CaseStudyIntern

 Name: UTKU GENC

 Note: I accidently set the bounds wrong for the levels, that is why it shows only 9 levels. To fix this you can do:
  - Assets/Scripts/GameScripts/Manager/GridManager
  - Line 100, change this "if (saveData.current_level >= 10)", to this "if (saveData.current_level > 10)"
