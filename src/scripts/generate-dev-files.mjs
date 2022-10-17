import fs from 'fs-extra';
import dotenv from "dotenv";
import { globby } from 'globby';
import path from 'path';

const cwd = process.cwd();
const scriptDir = path.dirname(process.argv[1]);

(async () => {
  dotenv.config();
  console.log(cwd);

  const launchSettingTemplate = await readFile(`${scriptDir}/templates/launchSettings.json`);
  const lambdaToolsDefaultsTemplate = await readFile(`${scriptDir}/templates/aws-lambda-tools-defaults.json`);

  // find all *.csproj
  const projectFiles = await globby("src/Proxy/(Commands|Events)/**/*.csproj", { cwd });
  console.log(projectFiles);
  for (let i = 0; i < projectFiles.length; i++) {
    const projectFile = projectFiles[i];
    const projectDir = path.dirname(`${cwd}/${projectFile}`);
    const projectPropsDir = path.resolve(`${projectDir}/Properties`);

    if (!fs.existsSync(projectPropsDir)) await fs.mkdir(projectPropsDir);

    const launchSettingPath = path.resolve(`${projectPropsDir}/launchSettings.json`)
    const lambdaToolsDefaultsPath = path.resolve(`${projectDir}/aws-lambda-tools-defaults.json`)

    console.log(projectDir);

    const launchSettingOutput = launchSettingTemplate
      .replace("{{DISCORD_BOT_TOKEN}}", process.env.BOT_TOKEN)
      .replace("{{S3_ASSET_BUCKET}}", process.env.APP_NAME + "-" + process.env.APP_ENVIRONMENT)
      .replace("{{ESP_AUTH_TOKEN}}", process.env.ESP_AUTH_TOKEN)
      .replace("{{BOT_DEV_CHANNEL_ID}}", process.env.BOT_DEV_CHANNEL_ID);

    const lambdaToolsDefaultsOutput = lambdaToolsDefaultsTemplate
      .replace("{{AWS_PROFILE}}", process.env.AWS_PROFILE)
      .replace("{{AWS_REGION}}", process.env.AWS_REGION);

    await writeFile(launchSettingPath, launchSettingOutput);
    await writeFile(lambdaToolsDefaultsPath, lambdaToolsDefaultsOutput);
  }

  console.log("[INFO] Done");
})();

async function readFile(filePath) {
  filePath = path.resolve(filePath);
  console.log(`[DEBUG][readFile]: ${filePath}`);
  return await fs.readFile(filePath, { encoding: "utf8" });
}

async function writeFile(filePath, data) {
  filePath = path.resolve(filePath);
  console.log(`[DEBUG][writeFile]: ${filePath}`);
  return await fs.writeFile(filePath, data);
}