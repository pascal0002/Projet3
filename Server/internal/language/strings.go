package language

var strings = []byte(`
en:
  error:
    channelNotFound: "The specified channel cannot be found."
    channelInvalidStart: "Invalid parameters, start must be the lowest parameter."
    channelInvalidUrl: "Invalid parameters, the url parameters must be a number."
    channelInvalidUUID: "Invalid channel ID. It must respect the UUID format."
    channelExists: "Channel already exists."
    wordBlank: "The word cannot be blank."
    wordBlacklist: "This word is not allowed!"
    wordInvalid: "This is not a word, please enter a valid word."
    difficultyRange: "The difficulty must be between 0 and 3."
    hintLimits: "The game must have at least 1 hint and not more than 10."
    hintEmpty: "The hints cannot be empty."
    hintWord: "The hint cannot contain the word."
    hintDuplicate: "The hint cannot be the same."
    invalidUUID: "A valid uuid must be set."
    gameNotFound: "The game cannot be found. Please check if the id is valid."
    gameFileInvalid: "The file is not valid"
    fileNotReadable: "The file cannot be read."
    fileInvalidType: "The file is not a valid type"
    ffaRound: "The number of round must be between 1 and 5 for the free for all game mode."
    soloCount: "The number of players must be one for the game mode solo."
    playerCount: "The number of players must be between 1 and %d."
    playerMax: "You cannot have more than %d players in a game."
    groupSingle: "You already have a group created you cannot create multiple groups."
    groupNotFound: "The group could not be found."
    usernameInvalid: "The username must be between 4 and 12 characters."
    usernameFail: "The user could not be created!"
    usernameExists: "The username already exists. Please choose a different username."
    firstNameInvalid: "Invalid first name or last name, it should not be empty."
    emailInvalid: "Invalid email, it must respect the typical email format."
    passwordInvalid: "Invalid password, it must have a minimum of 8 characters."
    sessionExists: "The user already has an other session tied to this account. Please disconnect the other session before connecting."
    userNotFound: "The specified user cannot be found."
    userUpdate: "The user could not be updated."
    modifications: "No modifications are found."
    groupOwner: "Only the group owner can kick people out."
    groupMembership: "The user does not belong to a group."
    userLeaveChannel: "User is not in the channel."
    userJoinChannel: "User is already joined to the channel."
    userJoinFirst: "The user needs to join the channel first."
    channelInvalidName: "The channel name is invalid."
    groupIsFull: "The group is full."
    userSingleGroup: "The user can only join one group."
    gameMinimum: "There are only %d game(s). There needs to be a minimum of 10 games before it can start."
    notEnough: "There are not enough users to start the game."
    passwordWrong: "The password is incorrect."
    loginBearer: "The username and the bearer must be set."
fr:
  error:
    channelNotFound: "Le canal spécifié n'a pu être trouvé."
    channelInvalidStart: "Paramètres invalides, start doit être celui le plus bas."
    channelInvalidUrl: "Paramètres invalides, les paramètres de l'url doivent être des nombres."
    channelInvalidUUID: "Identifiant de canal invalide. L'identifiant doit respecter le format UUID."
    channelExists: "Le canal existe déjà."
    wordBlank: "Le mot ne peut pas être vide."
    wordBlacklist: "Le mot n'est pas autorisé!"
    wordInvalid: "Ceci n'est pas un mot valide, veuillez entrer un mot valide."
    difficultyRange: "La difficulté doit être entre 0 et 3."
    hintLimits: "Le jeu doit avoir entre 1 et 10 indices maximums."
    hintEmpty: "Les indices ne peuvent être vides."
    hintWord: "L'indice ne peut pas contenir le mot à deviner."
    hintDuplicate: "L'indice ne peut être indentique aux autres."
    invalidUUID: "Un UUID valide doit être utilisé."
    gameNotFound: "Le jeu n'a pas été trouvé. Vérifiez si l'identifiant est valide."
    gameFileInvalid: "Le fichier n'est pas valide."
    fileNotReadable: "Le fichier ne peut être lu."
    fileInvalidType: "Le fichier n'est pas un type valide."
    ffaRound: "Le nombre de tours doit être entre 1 et 5 pour le type mêlée générale."
    soloCount: "Le nombre de joueurs doit être de un pour le mode de jeu solo."
    playerCount: "Le nombre de joueurs doit être entre 1 et %d."
    playerMax: "Vous ne pouvez pas avoir plus de %d joueurs dans une partie."
    groupSingle: "Vous avez déjà créé un groupe. Vous ne pouvez pas avoir plusieurs groupes à votre nom."
    groupNotFound: "Le groupe ne peut pas être trouvé."
    usernameInvalid: "Le nom d'utilisateur est doit être entre 4 et 12 caractères."
    usernameFail: "Le nom d'utilisateur n'a pu être créé!"
    usernameExists: "Le nom d'utilisateur existe déjà. Veuillez en choisir un autre."
    firstNameInvalid: "Prénom ou le nom est invalide. Il ne doit pas être vide."
    emailInvalid: "Courriel invalide, il doit respecter le format d'une adresse courriel."
    passwordInvalid: "Mot de passe invalide. Le mot de passe doit avoir un minimum de 8 caractères."
    sessionExists: "L'utilisateur possède déjà une session. Veuillez déconnecter l'autre session avant de réessayer."
    userNotFound: "L'utilisateur n'est pas trouvable."
    userUpdate: "L'utilisateur n'a pas été mis à jour."
    modifications: "Les modifications n'ont pas été trouvés."
    groupOwner: "Seulement le propriétaire du groupe peut retirer des joueurs."
    groupMembership: "L'utilisateur n'appartient pas à un groupe."
    userLeaveChannel: "L'utilisateur n'est pas dans le canal."
    userJoinChannel: "L'utilisateur est déjà présent dans le canal."
    userJoinFirst: "L'utilisateur doit rejoindre le canal avant."
    channelInvalidName: "Le nom du canal est invalide."
    groupIsFull: "Le groupe est plein."
    userSingleGroup: "L'utilisateur ne peut joindre qu'un seul groupe."
    gameMinimum: "Il n'a seulement que %d jeu(x). Un minimum de 10 jeux doivent être présents pour débuter la partie."
    notEnough: "Il n'a pas assez d'utilisateurs pour démarrer la partie."
    passwordWrong: "Le mot de passe est erroné."
    loginBearer: "Le nom d'utilisateur et le bearer doivent être dans la requête."
`)
