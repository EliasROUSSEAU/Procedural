<img src="Prj4_Jours1/Documentation/Projet4_JeuProcedural.png" width="80%">

---

**Elias ROUSSEAU**  
> Gaming Campus GTech3 Groupe GameBoy - 2025-2026  
> Semaine Théorique sur Unity - `Algorythme pour Jeu Procédurale`  

---

### Sommaires

- [Mise en place et Initialisation](#mise-en-place-et-initialisation)
- [SimpleRoomPlacement](#simpleroomplacement)
- [BSP2](#bsp2)
- [CellularAutomata](#cellularautomata)
- [NoiseGenerator](#noisegenerator)

---

### Mise en place et Initialisation

Important sinon le projet ne fonctionnera pas!  
Utilisation de **`UniTask`**:  
--> Guide d'installation ([**Lien UniTask OpenUPM**](https://openupm.com/packages/com.cysharp.unitask/#modal-manualinstallation))  

**1ère Etape**  
Sur Unity:  
- Onglet → Edit  
- Project Setting  
- Package Manager  

| Name  : `package.openupm.com`  
| URL   : `https://package.openupm.com`  
| Scope : `com.cysharp.unitask`  

<img src="Prj4_Jours1/Documentation/SetupUniTask_Unity_FullOnglet.png" width="50%">

**2ème Etapes**  
Une fois validé, fermer la fenêtre puis:  
- Onglet: Window  
- Package Manager  
- [+]  
- Name: `com.cysharp.unitask` | version: `2.5.10`  

<img src="Prj4_Jours1/Documentation/Unity_Package_Plus_Name.png" width="25%">
<img src="Prj4_Jours1/Documentation/Unity_Package_Plus_Name_InputField.png" width="25%">

**3ème Etapes:** (Facultatif, seulement si tu souhaite recommencer avec une base basique)  
Télécharger le package découverte de l'intervenant:  
[**LienDriveCampus**](https://drive.google.com/drive/folders/1QxmWzBSGsTq-miRODwUX_zA8UEcFaUDW)  
Nom du package: `ArchitectureProceduralGeneration.unitypackage`  
Glisser le package dans la Hierarchy Unity, puis importer le tout.

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
- On utilise RandomService() avec la Seed pour gérer l'aléatoire.  
- L'utilisation d'une Seed permet d'avoir du pseudo-aléatoire.  
- En changeant la Seed, on change le résultat. Si on réutilise la même Seed, on retrouvera le même résultat.  
- Utiliser toujours la même méthode de génération pour obtenir les mêmes décors et générations.

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

## BSP2

On utilise `ProceduralGridGenerator` avec le scriptableObject `New BSP_Correction`.  
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

Utilisation de `ProceduralGridGenerator` avec `New CellularAutomata_Correction`.  
Peu de paramètres :  
- `MaxSteps`  
- `GroundDensity`  
- `minGroundNeighbourCount`

<img src="Prj4_Jours1/Documentation/Cell_Auto/CellularAutomaton.png" width="20%">  
Exemple : la case rouge devient "Grass" si elle a ≥5 voisins "Grass".

**Étapes:**  
1. Remplir la grille aléatoirement Grass/Water.  
2. Créer une nouvelle grille selon `minGroundNeighbourCount`.  
3. Mettre à jour les cellules si elles changent.  
4. Répéter jusqu'à `MaxSteps`.

---

## NoiseGenerator

Utilisation de `ProceduralGridGenerator` avec `New Test_Noise_Perso`.  
Paramètres principaux :  
- `noiseType`, `frequency`, `amplitude`  
- `fractalType`, `octaves`, `lacunarity`, `persistence`  
- `waterHeight`, `sandHeight`, `grassHeight`, `rockHeight`

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
