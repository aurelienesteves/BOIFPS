# BOIFPS et son premier donjon

Document de conception initial — 16 novembre 2026.

BOIFPS est un roguelite d’horreur en vue subjective : le joueur traverse des salles de combat, récupère des améliorations qui transforment son arme et affronte un boss. Le déplacement et le tir doivent être immédiatement plaisants. L’horreur repose sur les lieux, les silhouettes et le son, tout en laissant les menaces lisibles pendant les combats rapides.

Le premier objectif est une partie complète dans un unique donjon avec cinq types d’ennemis ordinaires et un boss. Le périmètre inclut désormais un chargeur de six tirs, un ennemi volant, un ennemi mural et des galeries accessibles. Cette version doit permettre de juger le plaisir du combat avant d’étendre le jeu.

## Cadre de la première version

La demande fixe l’inspiration Binding of Isaac, l’horreur, le FPS rapide et le contenu du premier donjon. Les ajouts demandés étendent les trois ennemis initiaux à cinq et introduisent la recharge et la verticalité. Noms, thème, contrôles, chiffres et progression pourront évoluer après les essais.

| Élément | Proposition pour le prototype |
| --- | --- |
| Mode | Solo, PC, clavier et souris |
| Durée | 10 à 15 minutes pour une première victoire |
| Contenu | Un donjon, cinq ennemis, un boss, une arme, douze améliorations |
| Structure initiale | Salles faites à la main et parcours fixe ; compositions et récompenses variables |
| Échec | Mort définitive pour la partie, retour au départ, améliorations perdues |
| Victoire | Mort du boss, bilan et possibilité de relancer |
| Progression permanente | Aucune dans cette version |

## Piliers de jeu

- **Bouger pour survivre.** Se décaler, contourner et utiliser le dash permettent d’éviter les attaques. Rester immobile devient progressivement dangereux.
- **Construire son arme pendant la partie.** Quelques améliorations produisent des différences visibles de cadence, de portée ou de comportement des tirs.
- **Comprendre avant de subir.** Chaque attaque dangereuse possède un son, une animation ou une marque au sol. La pénombre ne doit pas masquer les projectiles.
- **Alterner tension et action.** Les transitions font monter l’inquiétude ; les arènes libèrent cette tension dans des combats rapides.

L’inspiration Isaac porte sur les salles, les récompenses et les combinaisons d’objets. Les personnages, décors, objets et attaques doivent avoir leur propre identité.

## Boucle de partie

1. Entrer dans une salle et identifier les menaces.
2. Les portes se ferment après le franchissement du seuil et une courte annonce de combat.
3. Éliminer les ennemis en utilisant le relief, les couverts et le dash.
4. Les portes se rouvrent quand tous les ennemis et renforts prévus sont morts.
5. Récupérer la récompense puis choisir la suite du parcours.
6. Combattre le boss et gagner, ou mourir et recommencer.

Les projectiles et zones dangereuses disparaissent à la fin du combat. Aucun retour d’ennemis dans une salle déjà nettoyée. La pause suspend toute la simulation.

## Joueur et combat

### Déplacements et commandes

| Action | Commande initiale | Comportement |
| --- | --- | --- |
| Se déplacer | ZQSD sur AZERTY, WASD sur QWERTY | Vitesse identique vers l’avant et sur les côtés |
| Regarder | Souris | Sensibilité réglable |
| Tirer | Clic gauche maintenu | Tir automatique |
| Recharger | R | Recharge anticipée ; automatique après le sixième tir |
| Dash | Maj | Impulsion dans la direction du déplacement, vers l’avant à l’arrêt |
| Saut | Espace | Saut avec contrôle en l’air, sans double saut |
| Interagir | E | Choisir une récompense ou utiliser la sortie |
| Pause | Échap | Reprendre, régler les options, recommencer, quitter |

Pas de sprint séparé ni d’accroupissement pour cette version. Le saut permet de franchir les tombeaux bas et d’éviter certaines attaques au sol ; la vitesse de base et le dash portent le rythme du jeu. La vitesse verticale initiale est de 9,8 m/s et la gravité de 28 m/s², pour une hauteur pratique d’environ 1,6 m. Une tolérance de 0,1 s au bord des obstacles et une mise en attente du saut de 0,12 s facilitent les enchaînements.

Valeurs de travail : déplacement à 8 m/s ; dash de 4 m en 0,18 s ; recharge de 1,2 s ; aucune invulnérabilité pendant le dash. Le dash respecte les murs et obstacles. Le champ de vision commence à 95° et reste réglable ; les secousses et le balancement de caméra peuvent être réduits.

### Santé et dégâts

Le joueur commence avec 100 PV, sans régénération automatique. Après un impact, une protection de 0,5 s empêche les dégâts en rafale. Un même contact avec un ennemi ne doit pas retirer des PV à chaque image.

Une petite réserve de soin rend 20 PV, dans la limite de 100. Les salles A et C accordent chacune un soin garanti ; l’antichambre du boss en contient un. Les soins restent au sol jusqu’à leur récupération. Les autres salles ordinaires peuvent en donner un avec une probabilité de 25 %.

### Arme de départ

**Le Cloueur** est une arme organique qui projette des pointes osseuses lumineuses. Le tir en projectile rend les améliorations et les esquives visibles.

| Paramètre | Valeur initiale à tester |
| --- | --- |
| Dégâts directs | 18 par pointe |
| Cadence | 6,25 tirs par seconde |
| Vitesse du projectile | 65 m/s |
| Portée | 40 m |
| Munitions | Chargeur de 6 cartouches, réserve illimitée |
| Recharge | 1,15 s, automatique à vide ou anticipée avec R |
| Précision | Tir central précis, sans dispersion aléatoire initiale |

Les murs arrêtent les tirs. Chaque coup produit une détonation grave, un éclair de bouche lumineux, un recul de l’arme et une impulsion de caméra. Le réticule s’écarte brièvement. Les impacts produisent des fragments, une déformation du corps et une petite poussée ; les morts ajoutent des débris et un son de rupture. Les secousses de caméra restent désactivables dans les options. Pas de dégâts de localisation dans le prototype.

Pendant la recharge, le joueur peut se déplacer, sauter et dasher. Le tir est bloqué jusqu’à la fin de l’animation ; une nouvelle pression sur R ne relance pas le compteur. La pause suspend la recharge. Les tirs doubles d’Os bifide consomment une seule cartouche. Le chargeur est plein au début de chaque nouvelle partie. Les dégâts par balle, les PV et les compositions sont réglés autour de cette alternance entre rafale et repositionnement.

## Donjon des Fosses de la Communion

Un ancien sanctuaire souterrain a été transformé en ossuaire vivant. Des conduits respirent dans les murs, les cloches résonnent sous terre et les autels semblent encore utilisés. Le joueur descend jusqu’à la créature qui entretient le lieu.

### Parcours

```text
Entrée → A → B → C → Reliquaire I → Carrefour → D → E → Antichambre → Boss
                                       |
                                       F → Reliquaire II
```

La branche F est facultative et se termine en impasse ; le joueur revient au carrefour par une salle nettoyée. Les deux reliquaires sont des salles sûres. Cela représente douze salles uniques, dont six arènes ordinaires, et un boss.

| Salle | Fonction | Composition initiale |
| --- | --- | --- |
| Entrée | Prendre en main déplacement, tir et dash | Cibles inertes, aucun ennemi |
| A | Introduire la poursuite et la recharge | 7 Rampants |
| B | Introduire la galerie et le vol | 3 Psalmodieurs en hauteur, 4 Rampants, 2 Séraphins |
| C | Introduire la charge, les murs et les renforts | 2 Pénitents, 1 Psalmodieur, 2 Tisse-murs ; puis 5 Rampants et 2 Séraphins |
| Reliquaire I | Première construction d’arme | Choix gratuit parmi 3 améliorations distinctes |
| Carrefour | Montrer la branche facultative | Aucun ennemi, signalétique vers le boss |
| D | Associer les cinq menaces et les fosses | 12 ennemis puis 4 renforts temporisés |
| E | Vérifier la maîtrise sur deux niveaux et autour du vide | 14 ennemis puis 4 renforts temporisés |
| F | Risque facultatif contre une amélioration | 5 Rampants, 2 Pénitents, 2 Séraphins, 2 Tisse-murs |
| Reliquaire II | Récompenser le détour | Choix gratuit parmi 3 améliorations distinctes |
| Antichambre | Préparer le boss | 1 soin, porte du boss clairement identifiable |
| Boss | Examen final des esquives | Le Carillonneur |

Dans C, la seconde vague commence 0,65 s après la mort du premier groupe, avec des marques d’apparition au sol. Dans D et E, des marques apparaissent 3,5 s après le premier groupe et annoncent quatre renforts 0,65 s plus tard. Les portes restent verrouillées tant que ces renforts sont attendus. L’annonce d’entrée dure 0,6 s. Les compositions fixes servent de référence ; leur variation vient après validation de cette version.

### Règles de construction des arènes

Le prototype utilise une enveloppe de 24 × 24 m par salle pour conserver les connexions. Les espaces de combat varient par leurs obstacles : nef avec deux tombeaux bas, autel central, crypte en croix, tombeaux décalés, ossuaires, cloison centrale et cercle rituel. Les couvertures basses se franchissent en sautant ; les ennemis les contournent. Une arène de boss plus grande pourra être étudiée après les essais.

La galerie du chœur B culmine à 2,6 m, avec deux rampes latérales. Dans E, deux tribunes à 2,4 m sont reliées par une passerelle et desservies par quatre rampes. Ces surfaces font partie de la navigation au sol : les poursuivants peuvent rejoindre le joueur à l’étage. Les sorties du donjon restent accessibles sans saut et les descentes ne causent pas de dégâts de chute.

D, E et F comportent chacune deux véritables ouvertures dans le plancher, signalées par des bordures orange. Des chemins de contournement restent accessibles au sol. Les fosses peuvent être franchies en sautant et la passerelle de E surplombe l’une d’elles. Tomber dans une fosse retire 20 PV et replace le joueur au dernier point sûr ; cette pénalité peut tuer. Les ennemis au sol évitent le vide et y interrompent leurs charges. Les repères de chemin et les bordures n’ajoutent aucun plancher invisible.

- Prévoir une boucle de déplacement continue et au moins deux sorties autour de chaque gros couvert.
- Garder des passages d’au moins 3 m de large et une zone libre derrière les portes.
- Éviter les cul-de-sac, les petits obstacles invisibles et les ennemis qui apparaissent derrière le joueur.
- Placer des couverts pleins contre les tirs, sans offrir un emplacement depuis lequel tous les ennemis deviennent inoffensifs.
- Faire apparaître tout renfort dans une zone visible annoncée pendant au moins une seconde.

Les portes restent repérables dans l’obscurité. Un ennemi coincé doit être détectable par les outils de développement ; aucun effacement arbitraire d’ennemi ne doit masquer un problème de navigation pendant les essais.

## Les cinq ennemis

Les silhouettes et sons doivent permettre de reconnaître chaque rôle avant de lire ses détails. Les ennemis n’infligent pas de dégâts à leurs alliés dans la première version.

| Ennemi | Rôle | PV | Déplacement | Dégâts |
| --- | --- | --- | --- | --- |
| Rampant | Contourner et poursuivre | 36 | 9,4 m/s, bond à 18 m/s | 12 par attaque |
| Psalmodieur | Contrôler la distance et les angles de tir | 72 | 5,2 m/s en repositionnement | 14 par projectile |
| Pénitent | Punir une trajectoire maintenue trop longtemps | 144 | 5,6 m/s, charge à 24 m/s | 26 par charge, 18 à proximité |
| Séraphin | Menacer depuis les airs et préparer un piqué | 54 | Vol à 7,5 m/s, piqué à 17 m/s | 12 par projectile, 14 par piqué |
| Tisse-mur | Déplacer l’attention vers les parois | 72 | Marche murale à 4,8 m/s | 16 par projectile |

Les ennemis au sol mémorisent la dernière position visible, choisissent des destinations praticables et utilisent les rampes pour changer d’étage. Un Rampant sur trois poursuit directement ; les autres visent des points latéraux à 4,5–7,5 m, puis referment leur approche. Leurs trajets sont calculés autour des obstacles et des fosses. Une partie des Pénitents vise la trajectoire anticipée du joueur. Les tireurs reculent de près et cherchent des angles décalés pour créer des tirs croisés. Les annonces restent visibles malgré un rythme plus rapide et des groupes plus nombreux.

### Le Rampant

Une silhouette maigre à quatre appuis, identifiable à ses griffes sur la pierre. À moins de 3,6 m, il annonce pendant 0,25 s un bond horizontal de 3,8 m dans une direction verrouillée. Il récupère pendant 0,28 s. Les contourneurs terminent leur déplacement latéral avant de bondir, sauf si le joueur arrive très près. Il évite de lancer son bond sur une cible située trop haut et cherche à rejoindre son étage.

**Réponse attendue :** se décaler, reculer ou le tuer rapidement. Il met la pression pendant que les autres ennemis contrôlent l’espace. Son corps seul n’inflige pas de dégâts.

### Le Psalmodieur

Un corps vertical suspendu à un reliquaire, avec une bouche éclairée. À moins de 17 m avec une ligne de vue, il annonce son tir pendant 0,38 s, puis lance deux salves de trois projectiles en éventail à 12,5 m/s. Les salves sont espacées de 0,16 s et visent la position du joueur au départ de chacune. Il cherche un nouvel angle pendant 0,65 s avant la prochaine préparation et tente de reculer lorsque le joueur arrive à moins de 6 m.

**Réponse attendue :** passer dans un intervalle ou utiliser un couvert. Il se repositionne si la ligne de vue reste bloquée ; il ne tire pas à travers les murs.

### Le Pénitent

Une masse humaine portant une cloche de pierre, reconnaissable à sa silhouette large. À portée de charge et sur un niveau comparable au joueur, il marque sa direction pendant 0,62 s. Il charge en ligne droite sur 12 m maximum, sans corriger sa trajectoire, puis récupère pendant 0,5 s. Un mur ou une fosse interrompt la charge. À proximité, il prépare pendant 0,34 s une frappe à 18 dégâts, suivie de 0,4 s de récupération.

**Réponse attendue :** esquiver latéralement puis profiter de l’ouverture. La charge ne peut toucher le joueur qu’une fois et traverse les petits alliés sans les pousser. Il doit disposer d’une trajectoire praticable avant de lancer sa préparation.

### Le Séraphin

Une créature ailée ocre qui orbite autour du joueur en maintenant une altitude comprise entre environ 2,6 et 4,8 m, selon le plafond. Elle évite les obstacles par des sondes de collision. Après une annonce de 0,5 s, elle tire un projectile à 14 m/s. Une attaque sur trois devient un piqué annoncé pendant 0,6 s, vers une position verrouillée ; la créature remonte ensuite avant de récupérer pendant 0,8 s.

**Réponse attendue :** viser en hauteur, interrompre la menace par le tir ou se décaler après l’annonce du piqué. Elle peut être touchée par toutes les pointes ordinaires.

### Le Tisse-mur

Une créature verte à six pattes qui adhère à une surface verticale réelle. Son corps s’oriente selon la normale du mur ; elle se déplace horizontalement et ajuste sa hauteur, tourne aux angles et inverse sa direction devant une interruption. Elle s’immobilise pendant une annonce de 0,55 s avant de cracher un projectile à 14 m/s, puis récupère pendant 0,9 s.

**Réponse attendue :** surveiller les parois et profiter de l’arrêt du tir pour viser. Ses PV, ses dégâts reçus et sa mort utilisent le même système que les ennemis au sol ; sa présence maintient les portes verrouillées.

## Boss du donjon

**Le Carillonneur** est un gardien massif dont la cage thoracique contient une cloche. Le combat réutilise les compétences apprises : déplacement latéral, lecture des projectiles et sortie des zones annoncées.

Valeurs actuelles : 2 200 PV ; durée visée de 60 à 100 s avec les améliorations et les recharges, à réévaluer après les essais. Aucun dégât de simple contact et aucun ennemi invoqué pour cette première version.

| Attaque | Annonce | Exécution | Réponse |
| --- | --- | --- | --- |
| Glas | Cloche qui s’allume pendant 0,6 s | Anneau de 16 projectiles à hauteur du joueur, vitesse 9,5 m/s ; 20 dégâts | Traverser un intervalle, sauter ou utiliser les piliers |
| Procession | Boss immobile, axe marqué pendant 0,65 s | Charge droite de 16 m maximum à 24 m/s ; 26 dégâts, puis récupération de 0,8 s | Dash latéral et contre-attaque |
| Sépulture | Trois cercles apparaissent au sol pendant 1 s | Explosion dans chaque cercle de 2 m de rayon ; 25 dégâts, puis disparition immédiate | Quitter les cercles ou être à plus de 1,1 m du sol lors de l’explosion |

Le boss reste vulnérable pendant les annonces et les récupérations. Les attaques ne se superposent pas. Les piliers arrêtent les projectiles et la charge ; les cercles ne peuvent pas occuper toute une voie de fuite.

**Phase 1, de 100 à 50 % de vie :** séquence Glas → Procession → Sépulture, avec 0,6 s de repos après chaque attaque, en plus de sa récupération éventuelle.

**Phase 2, sous 50 % :** transition sonore et visuelle de 1,1 s sans attaque. Le repos passe à 0,35 s et Glas produit deux anneaux espacés de 0,3 s, le second décalé d’un demi-intervalle. Les annonces conservent leur durée. La phase change seulement après la fin de l’attaque en cours ; aucune attaque n’est annulée ou déclenchée sans annonce.

La mort du boss interrompt toutes ses attaques, nettoie les dangers, ouvre la sortie et déclenche le bilan de victoire.

## Améliorations de partie

Un premier reliquaire est disponible dans l’entrée, puis deux autres sur le parcours. Chaque reliquaire propose trois objets distincts et permet d’en prendre un, avec au moins une nouvelle relique comportementale tant qu’il en reste. Un objet déjà acquis ne réapparaît pas pendant cette partie. Les modifications de comportement se cumulent ; les pourcentages de dégâts s’additionnent avant application.

| Objet | Effet proposé | Conséquence de jeu |
| --- | --- | --- |
| Nerf à vif | +25 % de cadence | Arme plus nerveuse |
| Dent creuse | +30 % de dégâts directs | Ennemis éliminés avec moins de touches |
| Os bifide | Deux pointes à ±3°, chacune à 65 % des dégâts directs | Meilleur rendement de près si les deux pointes touchent |
| Moelle brûlante | Une touche applique 3 dégâts/s pendant 3 s | Maintenir la pression en changeant de cible |
| Vertèbre percée | Chaque pointe traverse un ennemi supplémentaire | Aligner les groupes |
| Tendon noir | Recharge du dash réduite de 25 % | Davantage de mobilité |
| Chœur des morts | Une élimination libère trois esprits traqueurs à 8 dégâts | Nettoyer les groupes en concentrant d’abord le tir sur une cible |
| Pendule d’os | La dernière cartouche traverse et revient ; retour à 60 % des dégâts | Aligner les ennemis à l’aller et au retour |
| Sablier affamé | Pendant la recharge, vitesse des tirs hostiles proches réduite à 25 %, rayon 5 m | Se repositionner pendant les pauses de tir |
| Épine du vide | Trois mines par dash ; explosion après 0,55 s à 22 dégâts, rayon 2,5 m | Piéger les poursuivants en changeant de trajectoire |
| Paupière miroir | Le premier projectile reçu dans les 0,3 s après un dash est renvoyé | Utiliser le dash pour contre-attaquer un tireur |
| Satellite d’os | Une recharge à vide crée deux satellites durant 8 s, maximum quatre | Bloquer un projectile ou infliger 18 dégâts au contact par satellite |

La brûlure ne se cumule pas : une nouvelle touche rafraîchit sa durée. Les bonus de dégâts directs s’appliquent avant le coefficient d’Os bifide, qui dédouble aussi le Pendule en consommant une cartouche. Les esprits, mines, reflets et satellites ne génèrent pas de nouvelles chaînes d’esprits. Les satellites disparaissent après un contact et leur nombre est borné à quatre. Le Pendule respecte les murs et ne rend pas de munitions.

## Direction visuelle et sonore

Pierre humide, os, tissus liturgiques et chair discrètement intégrée à l’architecture forment la palette du donjon. Les zones de repos sont froides et presque silencieuses ; les combats réveillent les lumières, les respirations et les cloches.

Les projectiles hostiles possèdent un contour lumineux distinct des tirs du joueur. Le danger au sol combine forme, animation et son. Les effets d’impact restent courts pour ne pas remplir l’écran. Les sons d’attaque priment sur l’ambiance dans le mixage.

Le joueur doit ressentir l’horreur entre les salles et la maîtrise dans l’action. Pas de perte forcée du contrôle, de noir total pendant un combat ni de sursaut qui interrompt artificiellement la visée.

## Informations à présenter au joueur

Afficher les PV, le réticule, les cartouches restantes ou le temps de recharge, la disponibilité du dash, les améliorations acquises et la barre de vie du boss pendant son combat. Une indication directionnelle accompagne les dégâts reçus. Une carte simple distingue la salle actuelle, les salles visitées, les reliquaires et la porte du boss.

Le choix d’objet expose son nom, son effet chiffré et un aperçu compréhensible de son comportement. Le bilan présente victoire ou mort, durée, salles nettoyées et objets acquis. Le compteur de munitions reste visible pendant les combats.

## Découpage de réalisation

Le dépôt utilise Unity 6000.5.10f1 et référence URP, Input System et AI Navigation dans son manifeste au moment de la rédaction. Les éléments ci-dessous définissent l’ordre de réalisation et les conditions de validation.

| Étape | Livrable | Condition pour poursuivre |
| --- | --- | --- |
| 1 — Sensations | Une salle grise, contrôleur FPS, dash, arme et cible | Déplacements réactifs, tirs lisibles, collisions fiables |
| 2 — Combat | Les cinq ennemis, santé, dégâts et mort | Chaque attaque peut être comprise et évitée séparément |
| 3 — Salles | Portes, vagues, progression et récompenses | Une arène se verrouille et se termine sans blocage |
| 4 — Construction d’arme | Douze objets et trois choix de récompense sur le parcours complet | Chaque effet et les associations possibles fonctionnent |
| 5 — Donjon complet | Parcours, boss, défaite et victoire | Partie complète rejouable depuis le départ |
| 6 — Ambiance | Décors, sons, éclairage et retours d’impact | Les menaces restent lisibles dans la direction artistique |
| 7 — Réglages | Ajustement des chiffres et compositions | Durée et difficulté proches des cibles |

Prévoir des données séparées pour les réglages d’arme, d’ennemis, d’objets et de salles. Les systèmes nécessaires sont le contrôleur joueur, le combat, les comportements ennemis, le cycle des salles, les récompenses, l’état de partie et l’affichage. Un mode de développement permettra de relancer une salle, de choisir un objet et d’observer les attaques du boss.

## Critères de validation du premier donjon

- Une nouvelle partie mène de l’entrée au boss et à un écran de victoire ; mourir permet de recommencer avec l’état initial.
- Les cinq ennemis ont des silhouettes, annonces, attaques et réponses différentes ; le vol et la marche murale restent touchables et participent au verrouillage des salles.
- Six tirs déclenchent la recharge ; R permet de l’anticiper ; la pause suspend son temps restant.
- Le joueur et les ennemis au sol peuvent rejoindre les galeries et redescendre par les rampes.
- Toutes les attaques sont évitables au sol avec le déplacement et le dash, sans dégâts obligatoires.
- Les portes restent fermées pendant une vague et s’ouvrent après le dernier ennemi ; aucun renfort oublié ne bloque la progression.
- Le détour facultatif donne bien une amélioration supplémentaire et permet de rejoindre le chemin principal.
- Les objets produisent les effets décrits et leurs associations respectent les règles de cumul.
- Les deux phases du boss fonctionnent ; sa mort nettoie les dangers et déclenche une seule victoire.
- Le HUD, les réglages et le bilan sont lisibles ; les effets de caméra peuvent être réduits.
- La cible de 60 images/s sera mesurée sur une machine de référence à choisir avant la passe de performance.
- Les essais vérifient une première victoire autour de 10–15 minutes et recueillent les causes de dégâts jugées incompréhensibles.

## Limites de cette première version

La génération procédurale complète, les autres étages, les armes supplémentaires, les boutiques, les monnaies, les personnages jouables, la progression permanente, le multijoueur et la sauvegarde en cours de partie restent hors périmètre. La variété repose d’abord sur les combats, le détour facultatif et les deux choix d’amélioration.

La première décision à prendre après les essais sera de confirmer les sensations du déplacement et du tir. Si elles fonctionnent, l’étape suivante sera de varier les salles et leurs connexions en conservant les règles de lisibilité validées.

## Prototype disponible

Une première version du parcours est implémentée dans `Assets/BOIFPS/Scenes/FirstDungeon.unity`. Le [guide du prototype](Prototype.md) décrit le lancement, les commandes, le code et les vérifications. Les décors et personnages utilisent des formes simples ; les sons sont synthétiques. L’équilibrage, la durée cible et la direction artistique restent à valider en jeu. Le bilan apparaît directement à la mort du boss, sans interaction supplémentaire avec une sortie.
