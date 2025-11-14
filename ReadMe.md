<img src="Prj4_Jours1/Documentation/Projet4_JeuProcedural.png" width="80%">

---

**Elias ROUSSEAU**  
> Gaming Campus GTech3 Groupe GameBoy - 2025-2026  
> Semaine Théorique sur Unity - `Algorythme pour Jeu Procédurale`  

---

### Sommaires

- [Mise en place et Initialisation](#mise-en-place-et-initialisation)
- [SimpleRoomPlacement](#simpleroomplacement)
- [BSP](#bsp)
- [CellularAutomata](#cellularautomata)
- [NoiseGenerator](#noisegenerator)

---

### Mise en place et Initialisation

Important sinon le projet ne fonctionnera pas !  
Utilisation de **`UniTask`** :  
-> Guide d'installation : [**Lien UniTask OpenUPM**](https://openupm.com/packages/com.cysharp.unitask/#modal-manualinstallation)  

**1ère Étape**  
Sur Unity :  
- Onglet → Edit  
- Project Setting  
- Package Manager  

| Name  : `package.openupm.com`  
| URL   : `https://package.openupm.com`  
| Scope : `com.cysharp.unitask`  

<img src="Prj4_Jours1/Documentation/SetupUniTask_Unity_FullOnglet.png" width="50%">

---

**2ème Étape**  
Une fois appliqué, fermer la fenêtre et faire :  
- Onglet → Window  
- Package Manager  
- +  
- Name : `com.cysharp.unitask` | version : `2.5.10`  

<img src="Prj4_Jours1/Documentation/Unity_Package_Plus_Name.png" width="25%">
<img src="Prj4_Jours1/Documentation/Unity_Package_Plus_Name_InputField.png" width="25%">

---

**3ème Étape** *(Non obligatoire, seulement pour recommencer avec une base basique)*  
Télécharger le package découverte de UTKOWSKI Yona (intervenant) :  
[**LienDriveCampus**](https://drive.google.com/drive/folders/1QxmWzBSGsTq-miRODwUX_zA8UEcFaUDW)  
Nom du package : `ArchitectureProceduralGeneration.unitypackage`  
Glisser le package dans la **Hierarchy** Unity, puis importer le tout.

---

**FIN INITIALISATION**

Maintenant le projet contient les éléments de génération procédural :  
- SimpleRoomPlacement  
- BSP  
- Cellular Automata  
- Noise


---

**FIN INITIALISATION**

Ici, le projet contient plus d'éléments que le simple package de l'étape 3:  
- SimpleRoomPlacement  
- BSP  
- Cellular Automata  
- Noise  

---

### Informations Utiles

**SEED**:  
- Utilisation de `RandomService()` avec la Seed pour contrôler l'aléatoire. 
- L'utilisation d'une Seed permet d'avoir du un résultat pseudo-aléatoire (par exemple Minecraft).  
- Le changement de la Seed entrainera un résultat différent. L'utilisation d'une même Seed, on retrouvera le même résultat.  
- Toujours utilisé la même méthode de génération pour obtenir les mêmes générations.

---

## SimpleRoomPlacement

A l'ouverture du projet Unity, utiliser la scène `GridGenerator`.  
Sur le GameObject `ProceduralGridGenerator`, vérifier que la variable `GenerationMethod` utilise le scriptableObject `Simple Room Placement`.

<img src="Prj4_Jours1/Documentation/ProceduralGridGenerator_ScriptableObject_SimpleRoom.png" width="30%">

Si ce n'est pas le bon scriptableObject, le trouver dans:  
`Assets > Components > ProceduralGeneration > 0_SimpleRoomPlacement > SimpleRoomPlacement`  
Glisser/déposer dans l'inspector de `ProceduralGridGenerator` → `GenerationMethod`.  

**Étapes du ScriptableObject `Simple Room Placement.cs`**:  
1. Créer une `Room` de taille aléatoire (`minSizeX/Y` → `maxSizeX/Y`).  
2. Positionner la `Room` aléatoirement dans la grille.  
3. Vérifier si la `Room` chevauche une room déjà en place.  
4. Répéter les étapes 1 à 3 jusqu'à atteindre `MaxRooms` ou `MaxSteps`.  
5. Relier les rooms entre elles via le centre, en formant des couloirs "L" suivant l'ordre d'instanciation.

---

## BSP

Script utilisé `ProceduralGridGenerator` avec ScriptableObject `BSP2`.  
Libre à vous de tester les autres BSP.  

Rappel sur `Binary Tree` :  
<img src="Prj4_Jours1/Documentation/Screen_BSP/BinaryTree.png" width="30%">

**Étapes BSP (exemple):**  
1. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_0Split.png" width="20%"> Création de la grille mère `Root`.  
2. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_1Split.png" width="20%"> Création des `Sisters` (vertical/horizontal).  
3. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_2Split.png" width="20%"> Création des autres Sisters.  
4. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_3Split.png" width="20%"> Arrêt si découpe impossible ou steps max atteints.  
5. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_BuildRoom.png" width="20%"> Création des Rooms.  
6. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_BuildCorridor.png" width="20%"> Création des corridors "L".  
7. <img src="Prj4_Jours1/Documentation/Screen_BSP/Feuille1_BuildFinal.png" width="20%"> Résultat final.

---

## CellularAutomata

Scripts utilisé: `ProceduralGridGenerator` avec ScriptableObject `CellularAutomata`.  
### Paramètres du ScriptableObject

**Général**  
- `MaxSteps` : Nombre maximum d’itérations du Cellular Automata.  

**Initialisation**  
- `_noiseDensity` : Pourcentage de Cell Grass au départ.  

**Règles de transformation**  
- `_grassThreshold` : Nombre minimum de voisin Grass pour que une Cell devien Grass (0 → 8). 

<img src="Prj4_Jours1/Documentation/Cell_Auto/CellularAutomaton.png" width="20%">  
Exemple : la case rouge devient "Grass" si elle a ≥5 voisins "Grass".

**Étapes:**  
1. Remplir la grille procéduralement avec les Cell Grass et Water selon `_noiseDensity`.  
2. Création d'une grille selon `minGroundNeighbourCount`.  
3. Remplacement des Cell si condition remplie.  
4. Répéter le procèder jusqu' `MaxSteps`.

---

## NoiseGenerator

Scripts utilisé `ProceduralGridGenerator` avec ScriptableObject `NoiseGenerator`.
### Paramètres du ScriptableObject NoiseGenerator

**Noise Settings**  
- `noiseType` : type de bruit (ex : Perlin, Gradient, etc.)  
- `frequency` : fréquence du bruit (ex : 0.01 → 0.1)  
- `amplitude` : amplitude du bruit (ex : 0.5 → 1.5)  

**Fractal Settings**  
- `fractalType` : type de fractale (ex : FBm, Billow, etc.)  
- `octaves` : nombre de couches fractales (1 → 5)  
- `lacunarity` : écart entre les octaves (1 → 3)  
- `persistence` : influence de chaque octave (0.5 → 1)  

**Terrain Height Thresholds**  
- `waterHeight` : seuil de l'eau (-1 → 1, ex: 0.2)  
- `sandHeight` : seuil du sable (-1 → 1, ex: 0.3)  
- `grassHeight` : seuil de l'herbe (-1 → 1, ex: 0.6)  
- `rockHeight` : seuil des rochers (-1 → 1, ex: 1)


Exemple :  
<img src="Prj4_Jours1/Documentation/Noise/Exemple_Noise1.png" width="20%">  
<img src="Prj4_Jours1/Documentation/Noise/ConfigNoise_Exemple1.png" width="50%">  

**Génération de la grille avec le Noise:**  
1. Initialiser le type de bruit.  
2. Choisir la hauteur pour chaque type de cellule.  
3. Comparer aux seuils Water/Sand/Grass/Rock.  
4. Dessiner les cellules.  
5. Refaire pour toute la grille.  
6. Personaliser tout les paramètres pour le rendu souhaité.

---

Remerciements:  
Un merci à RUTKOWSKI Yona, intervenant de notre classe GTECH 3 pour son enseignement.
