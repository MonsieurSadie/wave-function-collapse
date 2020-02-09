# wave-function-collapse
Simple unity example of Wave Function Collapse in 2D (wip)


#UTILISATION
Le projet est sous la version 2018.4 de Unity
Deux scènes d'exemple sont fournies : road et ruins
Pour tester vos propres tilesets, dupliquez une de ces scènes


1ère étape :
a)importez un tileset (png) et laissez le en mode d'importantion Sprite>Single
L'option FilterMode doit être mise en mode "Point".
Dans la section Advanced, cochez "Read/Write Enabled"

b)Dans la scène, sélectionnez le game object TilesetParser.
Celui-ci a les paramètres suivants :
- Tilset : votre sprite de tileset
- Tile Width / Tile Height : la largeur/hauteur des tiles en pixels (par exemple 16x16)
- Color epsilon : la marge d'erreur tolérée lors de la comparaison de chaque pixels des bords des tiles(en pourcentage)
- Likeness min percent: le pourcentage minimum de pixels qui doivent être "identiques" (tient compte de la marge d'erreur précédente) pour que deux bords soient considérés comme pouvant être connectés (1 -> tous les pixels doivent être identiques)
- output filename : le nom du fichier qui contiendra les règles (celui-ci sera généré à la racine du dossier Assets)


2ème étape :
a) dupliquez votre tilset (ctrl+D) et importer ce duplicata en sprite multiple.
Faire un slicing par "CellSize" et sélectionnez la taille en pixel de vos tiles.

b) Dans la scène, sélectionnez le game object WFC
Celui-ci a les paramètres suivants :
- Tiles : tableau de Sprites contenant chaque tile de votre tileset
- Used Tiles: utile uniquement si vous ne voulez utiliser qu'une partie des tiles. Dans ce cas, remplissez le tabeau avec les ID des tiles que vous voulez utiliser. Laissez la taille à 0 sinon.
- Grid Width/Height: nombre de cases dans l'image générée
- constraints filename: nom du fichier contenant les règles, avec l'extension .txt
- tiles weights : permet de donner un poid différent pour chaque tile lorsqu'un choix aléatoire doit être fait. Si vous ne renseignez rien dans le tableau, chaque poid sera à 1.


# Remarques
Pensez à prendre des screenshots quand vous obtenez une composition intéressante.

Conseil : essayez au maximum d'avoir des bordures avec les mêmes pixels.
Dans tous les cas, utilisez d'abord un tileset réduit (par exemple 4 tiles) pour trouver les bonnes valeurs de paramètres du Tileset Parser (principalement pour le color epsilon et likeness). Cela vous permettra de voir quand vous avez des règles qui ne sont pas générées correctement.


