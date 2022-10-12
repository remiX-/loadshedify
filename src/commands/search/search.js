const axios = require('axios').default;

const { globalHandler } = require('../handler.js')
const { colours } = require("../../utils/colours");

exports.data = {
  name: 'search',
  type: 1,
  description: 'Replies with area information for a specific search',
  options: [
    {
      name: "area",
      description: "Search text",
      type: 3,
      required: true
    }
  ]
}

const action = async (body) => {
  const searchText = body.data.options[0].value;
  const user = body.member.user;
  const s3AssetBucket = process.env.S3_ASSET_BUCKET;
  const espAuthToken = process.env.ESP_AUTH_TOKEN;

  console.log("==== DEBUG ====");
  console.log(`searchText : ${searchText}`);
  console.log(`S3_ASSET_BUCKET: ${s3AssetBucket}`);
  console.log(`ESP_AUTH_TOKEN: ${espAuthToken}`);
  console.log(`User: ${JSON.stringify(user, null, 2)}`);
  console.log(JSON.stringify(body, null, 2));
  console.log("==== DEBUG ====");

  try {
    const { success, areas, error } = await getResultsFromEspApi(searchText);

    if (!success) {
      return {
        content: `Oops, request failed :/ - ${error}`
      }
    }

    console.log(JSON.stringify(areas, null, 2));

    const response = {
      content: `<@${user.id}>`,
      embeds: buildEmbeds({
        search: searchText,
        areas: areas,
        bucket: s3AssetBucket
      })
    };

    console.log(JSON.stringify(response, null, 2));

    return response;
  } catch (err) {
    console.log(`Something went wrong... ${err}`);

    return {
      content: `Something went wrong... ${err}`
    }
  }
}

async function getResultsFromEspApi(searchText) {
  const config = {
    headers: {
      Token: process.env.ESP_AUTH_TOKEN
    }
  }

  try {
    const result = await axios.get(`https://developer.sepush.co.za/business/2.0/areas_search?text=${searchText}`, config);

    return {
      success: true,
      areas: result.data.areas
    }
  } catch (err) {
    return {
      success: false,
      error: err
    }
  }
}

function buildEmbeds(data) {
  const embed = {
    title: `Results for ${data.search}`,
    description: `${data.areas.length} results`,
    // url: "http://example.com",
    timestamp: new Date().toISOString(),
    color: colours.LuminousVividPink,
    // provider: {
    //   name: "provider.name"
    // },
    // author: {
    //   name: "author.name",
    //   icon_url: "https://i.imgur.com/AfFp7pu.png",
    //   url: "https://discord.js.org"
    // },
    fields: [],
    footer: {
      icon_url: `https://${data.bucket}.s3.eu-west-1.amazonaws.com/assets/images/electricity.png`,
      text: "Developed by remiX",
    }
  };

  data.areas.forEach(element => {
    const id = element.id;
    const name = element.name;
    const region = element.region;

    embed.fields.push({
      name: "name",
      value: name,
      inline: true
    });

    embed.fields.push({
      name: "id",
      value: id,
      inline: true
    });

    embed.fields.push({
      name: "region",
      value: region,
      inline: true
    });
  });

  return [embed];
}

exports.handler = (event) => {
  globalHandler(event, action)
}
