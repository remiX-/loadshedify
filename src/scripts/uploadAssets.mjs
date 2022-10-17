// const AWS = require("aws-sdk");
// const fs = require("fs-extra");
// const chalk = require("chalk");
import AWS from "aws-sdk";
import chalk from "chalk";
import fs from "fs-extra";
import path from 'path';

const args = process.argv.slice(2);
const bucketName = args[0];
const scriptDir = path.dirname(process.argv[1]);

console.log(`Bucket: ${chalk.cyan(bucketName)}`);

AWS.config.region = "eu-west-1";

const s3 = new AWS.S3();

(async () => {
  const rootAssetsDir = `${scriptDir}/../assets`;

  let uploadPromises = [];

  try {
    const assetTypes = await fs.readdir(rootAssetsDir);

    for (const assetType of assetTypes) {
      const promises = await uploadAssets(rootAssetsDir, assetType);

      if (!promises) continue;

      uploadPromises = uploadPromises.concat(promises);
    }
  } catch (err) {
    console.log(chalk.red("FATAIL ERROR:"));
    console.log(chalk.red(` ${err}`));
    process.exit(1);
  }
})();

async function uploadAssets(assetDir, assetType) {
  const dir = `${assetDir}/${assetType}`;
  const files = await fs.readdir(dir);

  if (!files || files.length === 0) {
    console.log(chalk.red(`No assets found in ${assetDir}`));
    return;
  }

  let uploadPromises = [];

  console.log(`Uploading assets for ${chalk.yellow(assetType)}`);

  for (const key in files) {
    const fileName = files[key];
    const fileStream = fs.createReadStream(`${dir}/${fileName}`);
    const params = {
      Bucket: bucketName,
      ACL: "public-read",
      Key: `assets/${assetType}/${fileName.toLowerCase()}`,
      Body: fileStream,
      ContentType: "image/png",
      CacheControl: "max-age=86400"
    };

    console.log(` ${chalk.yellow("-")} ${chalk.blue(fileName)}`);

    const promise = s3.upload(params).promise();
    s3.putObject

    uploadPromises.push(promise);
  }

  return uploadPromises;
}