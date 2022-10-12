const path = require('path');
const axios = require('axios').default;
const fs = require('fs');

const loadCommands = async () => {
  let commandsOut = []

  const commandsPath = path.join(__dirname, '..', '..', 'src', 'commands')

  const commandModules = fs.readdirSync(commandsPath, { withFileTypes: true })
    .filter(dirent => dirent.isDirectory())
    .map(dirent => dirent.name)

  for await (const module of commandModules) {
    const commands = fs.readdirSync(`${commandsPath}/${module}`)
    for await (const command of commands) {
      if (!command.endsWith(".js")) continue;

      const { data } = require(`${commandsPath}/${module}/${command}`)
      commandsOut.push(data)
    }
  }

  return commandsOut
}

exports.register = async (appId, botToken, guildId) => {
  const commands = await loadCommands()
  const headers = {
    "Authorization": `Bot ${botToken}`,
    "Content-Type": "application/json"
  }

  const globalUrl = `https://discord.com/api/v8/applications/${appId}/commands`
  const guildUrl = `https://discord.com/api/v8/applications/${appId}/guilds/${guildId}/commands`
  endpoint = guildId ? guildUrl : globalUrl
  cmdInfo = guildId ? "Guild" : "Global"

  console.log(endpoint);
  console.log(JSON.stringify(commands, null, 2));

  axios.put(endpoint, JSON.stringify(commands), { headers: headers })
    .then(() => console.log(`${cmdInfo} Commands registered.`))
    .catch(e => console.log(e))
  return
}
