# Lancer le premier donjon

Ouvrir `Assets/BOIFPS/Scenes/FirstDungeon.unity` dans Unity puis lancer **Play**. Le bouton **Entrer dans les fosses** démarre la partie. Le menu **BOIFPS > Créer ou ouvrir le donjon** permet également d’ouvrir cette scène.

La scène construit son architecture au lancement et calcule sa navigation avant de créer le joueur. Les murs, personnages et sons sont des éléments de prototype. La graine du plan est affichée en pause.

## Commandes

| Action | Commande |
| --- | --- |
| Déplacement | ZQSD ou WASD selon le réglage du menu |
| Visée | Souris |
| Tir continu | Clic gauche maintenu |
| Recharge anticipée | R ; recharge automatique après le sixième tir |
| Dash | Maj gauche |
| Saut | Espace |
| Choix de relique | E près du reliquaire, puis clic sur un objet |
| Soin | Ramassage automatique à proximité si les PV sont incomplets |
| Pause et réglages | Échap |

Le menu de pause permet de changer la sensibilité, le champ de vision, le clavier et les secousses de caméra avec le balancement de l’arme. Les réglages s’appliquent à la session courante.

Le saut atteint environ 1,6 m et permet de franchir les tombeaux bas. Il conserve le contrôle en l’air, avec une courte tolérance au bord d’un obstacle et une mise en attente des pressions juste avant l’atterrissage. Il n’y a pas de double saut. Les explosions de Sépulture peuvent être évitées en étant suffisamment haut lors de leur résolution.

Le Cloueur tire à 6,25 coups/s, inflige 18 dégâts par pointe et propulse ses projectiles à 65 m/s. Son chargeur contient six cartouches, avec une réserve infinie. Après le sixième tir, une recharge de 1,15 s commence automatiquement ; R permet de l’anticiper. Le déplacement, le saut et le dash restent disponibles pendant la recharge. Os bifide transforme le tir en gerbe de cinq pointes pour une cartouche, à cadence réduite. Le compteur et le temps de recharge apparaissent dans le HUD ; la pause fige la recharge.

Les tirs déclenchent une détonation dédiée, un éclair lumineux, le recul de l’arme, une impulsion de caméra et l’expansion du réticule. Les impacts produisent des fragments et une petite poussée sur la cible. Les morts ont une rupture sonore et des débris supplémentaires.

## Armes et forge

Le premier reliquaire permet de choisir une arme pour la descente, de lui donner un adjectif, puis de prendre une relique. Les reliquaires suivants ajoutent chacun un adjectif encore disponible et une relique. Le nom complet de l’arme apparaît près des munitions et dans le menu pause.

| Arme | Manière de jouer |
| --- | --- |
| Cloueur | Tir précis et rapide. Maintenez la pression sur une cible. |
| Diapason | À l’impact, un arc touche un second ennemi visible à moins de 5 m. |
| Semeur | Plantez une graine dans le décor ou sur une cible. Elle éclate après 0,7 s dans un rayon de 3 m. |
| Comète en cage | Lâche un noyau lent qui accélère en vol. |
| Croissant de suie | Décrit un arc horizontal, alternativement à gauche et à droite. |
| Caducée | Deux serpents ondulent en sens opposés le long du viseur. |
| Sextant des veilleurs | Trois aiguilles convergent à 6 m puis se séparent. |
| Orgue des côtes | Alternez entre une nappe horizontale et une salve verticale. |
| Égide errante | Projette un disque qui efface le premier tir hostile rencontré. |
| Fuseau du vide | Traverse trois cibles et gagne en puissance avec la distance. |
| Chapelet brisé | La dernière perle libère une couronne de douze tirs autour de vous. |
| Calice affamé | Une mise à mort directe rend 5 PV. Visez les ennemis affaiblis. |
| Pilonnier du glas | Un impact direct ébranle les autres ennemis visibles à 3 m. |

Les pages de la forge présentent les adjectifs encore disponibles. Leurs effets se cumulent avec les reliques équipées.

| Adjectif | Effet |
| --- | --- |
| Vif | Réduit de 20 % le délai entre deux attaques. |
| Réverbérant | Les tirs rebondissent une fois sur le décor avant de poursuivre leur course. |
| Aimanté | Les tirs dévient vers un ennemi visible devant eux. |
| Ardent | Les impacts directs embrasent les cibles. |
| Perforant | Traverse une cible de plus. |
| Allongé | Portée aller augmentée de 50 %. |
| Fulgurant | Vitesse initiale des projectiles augmentée de 40 %. |
| Massif | Double la largeur des projectiles et leur zone de contact. |
| Aérien | Tirer en vol augmente les dégâts de 35 %. |
| Ancré | Tirer presque immobile augmente les dégâts de 30 %. |
| Intrépide | Sous 40 PV, les tirs infligent 50 % de dégâts supplémentaires. |
| Inaugural | La première cartouche du chargeur inflige 60 % de dégâts supplémentaires. |
| Terminal | La dernière cartouche inflige 80 % de dégâts supplémentaires. |
| Hâtif | Recharge 30 % plus vite. |
| Bondissant | Réduit de 25 % la récupération du dash. |
| Siphonnant | Une élimination directe rend 2 PV. |
| Impitoyable | Inflige 45 % de dégâts supplémentaires aux ennemis sous la moitié de leurs PV. |
| Chasseur | Inflige 35 % de dégâts supplémentaires aux cibles volantes ou murales. |
| Brutal | À moins de 4 m de trajet, les impacts gagnent 40 % de dégâts. |
| Lointain | Après 8 m de trajet, les impacts gagnent 50 % de dégâts. |
| Mordant | Sur une cible déjà en feu, inflige 40 % de dégâts supplémentaires. |
| Jumeau | Ajoute une pointe latérale à 55 % de puissance à chaque attaque. |
| Relanceur | Chaque élimination directe retranche 0,4 s au délai du dash. |

## Parcours jouable

Le plan comprend 11 à 15 salles. Une route principale relie l’entrée, un reliquaire, l’antichambre et le boss. Des branches peuvent se ramifier à leur tour et un second reliquaire récompense l’exploration. Les portes et la carte suivent les connexions générées. Chaque arène se verrouille à l’entrée. Le nombre total de combats dépend du plan. Dans l’inspecteur de DungeonGame, une graine différente de zéro reproduit le plan et ses dimensions. Zéro tire une graine à chaque nouvelle partie. Les offres et les soins utilisent leur propre tirage.

Une salle ferme ses portes lorsque le joueur pénètre suffisamment à l’intérieur. Dans C, une seconde vague annoncée au sol arrive après la mort du premier groupe. Le Carillonneur change de rythme sous la moitié de ses PV. La victoire intervient à sa mort ; la défaite à zéro PV. Une nouvelle descente remet à zéro la santé, les salles et les reliques.

## Arènes et difficulté

Les salles conservent des portes alignées, mais possèdent des dispositions de couvert différentes. Les tombeaux bas peuvent être franchis par le joueur ; les ennemis les contournent grâce à la navigation.

| Salle | Disposition | Rencontre |
| --- | --- | --- |
| A | Nef, tombeaux bas et balcon à 3 m avec deux rampes | 7 Rampants |
| B | Autel, galerie à 2,6 m et deux rampes latérales | 3 Psalmodieurs, 4 Rampants, 2 Séraphins |
| C | Crypte en croix et pont central à 2,6 m | 2 Pénitents, 1 Psalmodieur, 2 Tisse-murs ; puis 5 Rampants et 2 Séraphins |
| D | Tombeaux, deux fosses et tribune reliée à un pont à 3,2 m | 12 ennemis au départ, puis 4 renforts temporisés |
| E | Deux tribunes, passerelle, quatre rampes et deux fosses | 14 ennemis au départ, puis 4 renforts temporisés |
| F | Cloison, deux fosses, balcons et passerelle à 2,8 m | 5 Rampants, 2 Pénitents, 2 Séraphins, 2 Tisse-murs |
| Boss | Cercle rituel, piliers et deux tribunes à 2,8 m | Carillonneur à 2 200 PV |

Les Rampants approchent à 9,4 m/s. Un sur trois poursuit directement ; les autres cherchent des points latéraux à 4,5–7,5 m du joueur, par un côté gauche ou droit, avant de refermer leur approche. Ils cherchent des chemins praticables autour des obstacles et des fosses. Certains Pénitents visent la trajectoire anticipée du joueur ; les autres poursuivent directement. Les tireurs reculent de près et cherchent des positions décalées pour créer des tirs croisés.

Les annonces et récupérations sont plus courtes : bond des Rampants annoncé pendant 0,25 s, charge des Pénitents pendant 0,62 s et tirs des Psalmodieurs pendant 0,45 à 0,55 s selon le motif. Les tirs se déplacent à 10–18 m/s selon le motif pour les Psalmodieurs et à 14 m/s pour les créatures hors sol. Le groupe vise une limite de quatre attaquants engagés simultanément. Les PV restent identiques à la version précédente : le rythme vient du nombre et de la vitesse.

Le **Séraphin** vole autour du joueur, tire après une annonce et remplace un tir sur trois par un piqué vers une position verrouillée. Le **Tisse-mur** se déplace sur les surfaces verticales : son orientation suit la normale du mur, il tourne aux angles et inverse son trajet devant une rupture de paroi. Il prépare ses tirs depuis les côtés de l’arène. Les deux créatures peuvent être touchées par les projectiles ordinaires et comptent dans le verrouillage des portes.

Dans D et E, les renforts sont annoncés par des marques lumineuses 3,5 s après le premier groupe, puis arrivent 0,65 s plus tard. Les portes restent fermées tant que ces renforts sont attendus. La seconde vague de C conserve également une annonce au sol.

Les parcours en hauteur des sept arènes sont accessibles par des rampes physiques, intégrées au NavMesh. Le joueur et les ennemis au sol peuvent monter et redescendre ; les tribunes de E sont reliées par une passerelle. Le niveau ne demande aucun saut pour terminer le parcours.

Les fosses de D, E et F sont de véritables ouvertures dans le sol, bordées de marques orange. On peut les contourner, les franchir en sautant ou utiliser les passages en hauteur. Une chute coûte 20 PV et replace le joueur au dernier point sûr ; elle peut tuer si les PV sont insuffisants. Les ennemis au sol les contournent et interrompent leurs charges devant le vide. Les soins de fin de salle sont déposés sur un endroit sûr.

## Reliques et combinaisons

Les six objets initiaux restent disponibles. Six objets supplémentaires modifient les tirs, le dash et les recharges.

| Relique | Effet |
| --- | --- |
| Chœur des morts | Une élimination libère trois esprits à tête chercheuse, chacun à 8 dégâts |
| Pendule d’os | Toutes les cartouches traversent et reviennent vers le joueur ; le déplacement du joueur change la trajectoire du retour |
| Sablier affamé | Pendant la recharge, les projectiles hostiles dans un rayon de 5 m passent à 25 % de leur vitesse |
| Épine du vide | Le dash dépose trois mines ; elles explosent après 0,55 s pour 22 dégâts dans un rayon de 2,5 m |
| Paupière miroir | Pendant 0,55 s après le dash, capture un projectile sans dégâts ; la prochaine attaque inflige ×2,5 et traverse un ennemi supplémentaire |
| Satellite d’os | Une recharge commencée à vide crée deux os en orbite pendant 8 s ; chaque os absorbe un projectile ou inflige 18 dégâts au contact, puis disparaît |

Os bifide produit aussi cinq pointes revenantes avec Pendule, tout en consommant une seule cartouche. Le miroir renforce toute la gerbe et le feu reste applicable sur les deux trajets. Les mines profitent des déplacements rapides du dash. Le Sablier protège les pauses de tir dans les groupes denses. Le nombre de satellites est plafonné à quatre et les dégâts secondaires ne déclenchent pas de nouveaux esprits, ce qui borne les chaînes d’effets. Les projectiles revenants respectent les murs et ne remboursent pas de munitions.

| Ennemi | PV | Tirs directs de base pour le tuer |
| --- | --- | --- |
| Rampant | 36 | 2 |
| Psalmodieur | 72 | 4 |
| Pénitent | 144 | 8 |
| Séraphin | 54 | 3 |
| Tisse-mur | 72 | 4 |

Les dégâts et effectifs ont été ajustés pour laisser des occasions de se repositionner pendant la recharge. Le boss conserve ses deux phases et dispose de pauses légèrement plus longues entre ses attaques. La difficulté réelle reste à évaluer en jeu.

Un soin est garanti après A, après C et dans l’antichambre. Les autres salles ordinaires ont 25 % de chances d’en donner un. La protection du joueur après un impact dure 0,5 s.

## Organisation du code

| Script | Responsabilité |
| --- | --- |
| `DungeonGame` | Construction du niveau, navigation, matériaux, état de partie et sons |
| `DungeonPlayer` | Caméra, déplacement, dash, arme, santé et application des reliques |
| `DungeonRoom` | Déclenchement, compositions, vagues, portes et ramassages |
| `DungeonEnemy` | Santé commune, navigation tactique, annonces et attaques au sol |
| `DungeonSurfaceEnemy` | Vol, piqués, adhérence aux parois et attaques des deux nouvelles créatures |
| `DungeonRelics` | Esprits, mines, ralentissement local et satellites |
| `DungeonProjectile` | Détection des impacts sur le trajet et zones de Sépulture |
| `DungeonHud` | HUD, carte, accueil, pause, récompenses et bilan |
| `DungeonFeedback` | Fragments temporaires lors des impacts et des morts, avec un plafond de 100 |
| `DungeonSetup` | Création et ouverture de la scène depuis l’éditeur |
| `DungeonSmokeCheck` | Vérifications automatiques en Play, uniquement dans l’éditeur |
| `DungeonCombatCheck` | Vérification du saut, du tir, des réactions aux impacts et du franchissement des tombeaux |
| `DungeonExpansionCheck` | Chargeur, recharge, pause, navigation en hauteur et nouveaux ennemis |
| `DungeonHordeCheck` | Fosses, chute, contournements, six nouvelles reliques et limites des effets |

Les scripts se trouvent dans `Assets/BOIFPS`. Les chiffres sont pour l’instant réglés dans les scripts ; leur extraction vers des données éditables constituera une prochaine étape de production.

## Vérification automatique

En Play, sur une partie neuve arrêtée à l’accueil, ajouter temporairement `DungeonSmokeCheck` à un GameObject vide. Il pilote une partie de contrôle : navigation, compositions, portes, dégâts de projectile, vague supplémentaire, choix d’objet, cycle du boss et victoire. Il soigne automatiquement le joueur pendant ces vérifications et détruit son propre GameObject à la fin.

Le rapport est écrit dans `Artifacts/smoke-check.txt`. Ce contrôle utilise des déplacements instantanés et des éliminations forcées pour vérifier les transitions. Il ne valide ni la difficulté, ni le plaisir de jeu, ni l’équilibrage des objets. Un essai manuel reste nécessaire pour ces points.

Sur une autre partie neuve, `DungeonCombatCheck` teste le saut et les nouveaux effets de combat. Son rapport est écrit dans `Artifacts/combat-check.txt`. Recommencer la partie après ce contrôle avant de jouer ou de lancer le contrôle du parcours.

Sur une partie neuve séparée, `DungeonExpansionCheck` vérifie les six cartouches, la recharge automatique et manuelle, le blocage du tir pendant la recharge, la pause, Os bifide, les chemins vers les galeries, la montée physique d’une rampe et les déplacements des deux nouveaux ennemis. Son rapport est écrit dans `Artifacts/expansion-check.txt`.

`DungeonHordeCheck`, également sur une partie neuve séparée, vérifie les ouvertures dans le sol, la récupération après chute, les approches latérales et les six nouvelles reliques. Son rapport est écrit dans `Artifacts/horde-check.txt`. Le contrôle du parcours vérifie également les renforts temporisés et le maintien des portes fermées.

## État de cette version

Le prototype couvre le parcours complet et ses systèmes principaux. Il utilise des formes simples, des couleurs et des impulsions sonores synthétiques. La durée réelle des parties, les performances sur une machine de référence, la direction artistique définitive et le confort du combat doivent encore être évalués en jouant.

La graine fixe le parcours et les types de rencontres. La sortie finale est représentée par le bilan de victoire. La progression dure le temps de la partie.

## Verticalité, attaques et retours des reliques — 17 novembre 2026

Les sept arènes proposent désormais des parcours en hauteur sans saut obligatoire. A reçoit un balcon à 3 m et deux rampes. C reçoit un pont longitudinal à 2,6 m, accessible des deux côtés. D offre une tribune à 3,2 m reliée à un pont au-dessus de la zone des fosses. F propose deux balcons à 2,8 m et une passerelle derrière la cloison. Le boss dispose de deux tribunes à 2,8 m, chacune avec deux accès. B et E conservent leurs galeries. Les voûtes de combat sont relevées à 8 m. Les bords dorés et turquoise signalent les plateformes. Les apparitions au sol et les points de retour prennent en compte les nouveaux reliefs.

Les Rampants alternent bonds et balayages frontaux annoncés. Les Psalmodieurs alternent éventail double, rafale de trois projectiles vers une direction verrouillée et salve verticale. Les Pénitents disposent d’une Sépulture ciblée, notamment contre les joueurs sur les galeries : le disque orange verrouille la position avant de frapper après 0,95 s. Le déplacement ou un saut permet de l’éviter. Les frappes du boss prennent aussi en compte l’étage visé ; une frappe au sol ne touche pas le balcon. Les Tisse-murs alternent tir simple et éventail. Les Séraphins peuvent voler plus haut dans les arènes.

Chaque acquisition affiche le nom et l’effet pendant quatre secondes. Les activations des six reliques de comportement affichent des messages temporaires avec une couleur propre, accompagnés de cercles dans le monde. Les esprits, les pointes du Pendule et les projectiles brûlants ont des couleurs distinctes. Les ennemis en feu restent orangés. Le miroir indique son état armé dans le HUD et confirme la capture et la riposte stockée ; les satellites annoncent leur nombre, leur durée et les absorptions. Les descriptions des objets équipés sont consultables en pause. Les messages répétés identiques sont limités, sans masquer un changement d’état.

`DungeonDepthCheck` vérifie les sept chemins en hauteur, le support physique des plateformes, les frappes par étage, l’esquive par saut, les retours de plusieurs reliques et le cycle des trois tirs. `DungeonVerification.Run` le lance dans un éditeur séparé en batch ; rapport : `Artifacts/depth-check.txt`.

Validation du 17 novembre : compilation C# réussie, 43 contrôles de verticalité/attaques/reliques et 146 contrôles du parcours complet réussis dans une copie isolée du projet sous Unity 6000.5.10f1, en batch sans rendu. La lisibilité visuelle et l’équilibrage restent à apprécier en Play. Rapports : `Artifacts/depth-check.txt` et `Artifacts/smoke-check.txt`.

## Quatre transformations de combat

Le premier reliquaire donne immédiatement une identité de combat, sans proposer de simple bonus de statistiques. Les quatre transformations se combinent ; les bonus de cadence, de dégâts ou de pénétration continuent de les soutenir.

| Transformation | Réglage | Décision encouragée |
| --- | --- | --- |
| Os bifide | 5 pointes de 14 dégâts, éventail de ±14°, intervalle 0,55 s, portée 10 m et atténuation jusqu’à 35 % | Se rapprocher et choisir une ouverture avant de tirer |
| Pendule d’os | Chaque pointe traverse et revient après 11 m ; vitesse aller 30 m/s, retour 22 m/s ; un impact par ennemi et par trajet | Se déplacer pour aligner les ennemis sur le retour |
| Moelle brûlante | 6 dégâts/s pendant 4 s ; une mort embrasée inflige 14 dégâts et embrase les cibles visibles à 4 m dans la même salle | Préparer une victime au milieu d’un groupe pour propager le feu |
| Paupière miroir | Capture d’un projectile pendant 0,55 s après le dash ; riposte stockée jusqu’au prochain tir, ×2,5 dégâts et une pénétration supplémentaire | Traverser une menace pour préparer une attaque puissante |

La capture ne renvoie plus automatiquement de projectile. Le HUD conserve « RIPOSTE ×2,5 PRÊTE » jusqu’au tir, qui consomme la charge pour toute l’attaque, y compris une gerbe. Une charge déjà stockée empêche d’en capturer une autre. Le rechargement conserve la charge. Le miroir se combine avec le Pendule : le bonus reste attaché aux pointes pendant leur retour.

Avec Os bifide + Pendule, la portée aller reste de 10 m et l’atténuation liée à la distance s’applique également au retour. Les retours respectent les murs et ne rendent pas de cartouches. Le feu peut se propager par des morts provoquées par le feu lui-même ; chaque ennemi ne meurt qu’une fois, les explosions sont traitées par groupes de quatre par frame et la file est vidée à la fin du combat. Les murs bloquent la propagation.

`DungeonBuildCheck`, lancé avec `BOIFPS.DungeonVerification.Builds` dans un éditeur batch séparé, vérifie les offres initiales, les différences de dégâts près/loin, la cadence et la portée, les deux trajets du Pendule, la propagation et ses obstacles, la capture réelle d’un projectile, la riposte pénétrante, la combinaison des quatre reliques et la pause. Rapport : `Artifacts/build-check.txt`.

Validation des quatre transformations : 51 contrôles de builds et 146 contrôles du parcours complet réussis sous Unity 6000.5.10f1 en batch sans rendu. Le test de proximité mesure environ 66 dégâts pour une gerbe à 2 m contre 8 à 8 m sur la même cible. Le ressenti, la lisibilité en mouvement et l’équilibrage doivent encore être appréciés en Play.
