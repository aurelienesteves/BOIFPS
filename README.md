# BOIFPS — Les Fosses de la Communion

Prototype solo de roguelite d’horreur en vue subjective : un donjon procédural de 11 à 15 salles, cinq types d’ennemis ordinaires, un boss et quatorze reliques combinables.

## Lancer le jeu

1. Ouvrir ce dossier avec **Unity 6000.5.10f1** et laisser Unity restaurer les packages.
2. Ouvrir `Assets/BOIFPS/Scenes/FirstDungeon.unity` (ou le menu **BOIFPS > Créer ou ouvrir le donjon**).
3. Lancer **Play**, puis **Entrer dans les fosses**.

ZQSD/WASD : déplacement ; souris : viser/tirer ; Espace : saut ; Maj : dash ; R : recharge ; E : reliquaire ; Échap : pause. Le clavier peut être changé dans le menu.

Au premier reliquaire, parcourez les 13 armes puis choisissez l’un des 23 adjectifs de la forge. Le Sextant croise trois aiguilles à six mètres, l’Égide peut intercepter un tir et le Chapelet réserve sa salve circulaire à la dernière cartouche. Aux reliquaires suivants, un nouvel adjectif vient compléter les précédents. Chaque choix de forge est suivi d’un choix de relique. Le catalogue complet figure dans la documentation du prototype.

Le premier choix de relique propose trois styles parmi la gerbe à courte portée, les pointes revenantes, le feu qui se propage et la capture de projectiles pour préparer une riposte. Les reliques suivantes peuvent combiner leurs effets. Le plan, les embranchements et la répartition des arènes sont tirés à chaque descente. Le Cloître suspendu et les Escaliers du beffroi ont des galeries de hauteur, d’écartement et de profondeur variables. La Plume du clocher donne un second saut et le Poumon de cendre transforme les cartouches restantes en dégâts de proximité lors d’une recharge anticipée. La graine du plan figure en pause et peut être rejouée en renseignant Dungeon Seed sur DungeonGame dans l’inspecteur avant Play.

## Documentation et vérification

- [Prototype, règles et commandes](Docs/Prototype.md)
- [Conception initiale](Docs/Conception.md)
- [Contrôles des transformations](Artifacts/build-check.txt).
- [Contrôles du parcours complet](Artifacts/smoke-check.txt).
- [Contrôles de verticalité](Artifacts/depth-check.txt).

Les vérifications automatiques ont été exécutées dans une copie séparée du projet en batch sans rendu. Le ressenti et l’équilibrage restent à apprécier en Play. Les scripts de contrôle se trouvent dans `Assets/BOIFPS/Runtime` et les points d’entrée batch dans `Assets/BOIFPS/Editor/DungeonVerification.cs`.

Les visuels et les sons sont ceux d’un prototype. L’architecture et le NavMesh sont construits au lancement. Les caches Unity et les fichiers propres à la machine sont exclus du dépôt.
