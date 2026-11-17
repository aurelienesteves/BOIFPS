# BOIFPS — Les Fosses de la Communion

Prototype solo de roguelite d’horreur en vue subjective : un donjon de sept arènes, cinq types d’ennemis ordinaires, un boss et douze reliques combinables.

## Lancer le jeu

1. Ouvrir ce dossier avec **Unity 6000.5.10f1** et laisser Unity restaurer les packages.
2. Ouvrir `Assets/BOIFPS/Scenes/FirstDungeon.unity` (ou le menu **BOIFPS > Créer ou ouvrir le donjon**).
3. Lancer **Play**, puis **Entrer dans les fosses**.

ZQSD/WASD : déplacement ; souris : viser/tirer ; Espace : saut ; Maj : dash ; R : recharge ; E : reliquaire ; Échap : pause. Le clavier peut être changé dans le menu.

Le premier reliquaire propose trois styles parmi la gerbe à courte portée, les pointes revenantes, le feu qui se propage et la capture de projectiles pour préparer une riposte. Les reliques suivantes peuvent combiner leurs effets. Les arènes comportent des galeries, des rampes, des passerelles et des fosses.

## Documentation et vérification

- [Prototype, règles et commandes](Docs/Prototype.md)
- [Conception initiale](Docs/Conception.md)
- [Contrôles des transformations](Artifacts/build-check.txt) : 51 réussis.
- [Contrôles du parcours complet](Artifacts/smoke-check.txt) : 146 réussis.
- [Contrôles de verticalité](Artifacts/depth-check.txt).

Les vérifications automatiques ont été exécutées dans une copie séparée du projet en batch sans rendu. Le ressenti et l’équilibrage restent à apprécier en Play. Les scripts de contrôle se trouvent dans `Assets/BOIFPS/Runtime` et les points d’entrée batch dans `Assets/BOIFPS/Editor/DungeonVerification.cs`.

Les visuels et les sons sont ceux d’un prototype. L’architecture et le NavMesh sont construits au lancement. Les caches Unity et les fichiers propres à la machine sont exclus du dépôt.
