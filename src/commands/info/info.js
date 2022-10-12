const axios = require('axios').default;

const { globalHandler } = require('../handler.js')
const { colours } = require("../../utils/colours");
const { days, daysByNumber } = require("../../utils/days");

exports.data = {
  name: 'info',
  type: 1,
  description: "Gets load shedding schedule for current or specified day",
  options: [
    {
      name: "id",
      description: "ESP specific id",
      type: 3,
      required: true
    },
    {
      name: "day",
      description: "Day to retrieve schedule for",
      type: 3,
      required: false
    }
  ]
}

const action = async (body) => {
  const id = body.data.options[0].value;
  let day = "";
  let today = daysByNumber[new Date().getDay()];

  try {
    day = body.data.options[1].value;

    if (days.indexOf(day) === -1) {
      console.log(`invalid day: ${day}, default to today (${today})`);
      day = today;
    }
  } catch (ex) {
    console.log(`no day specified, using today (${today})`);
    day = today;
  }

  const user = body.member.user;
  const s3AssetBucket = process.env.S3_ASSET_BUCKET;
  const espAuthToken = process.env.ESP_AUTH_TOKEN;

  console.log("==== DEBUG ====");
  console.log(`id: ${id}`);
  console.log(`day: ${day}`);
  console.log(`S3_ASSET_BUCKET: ${s3AssetBucket}`);
  console.log(`ESP_AUTH_TOKEN: ${espAuthToken}`);
  console.log(`User: ${JSON.stringify(user, null, 2)}`);
  console.log(JSON.stringify(body, null, 2));
  console.log("==== DEBUG ====");

  try {
    const { success, data, error } = await getResultsFromEspApi(id);

    if (!success) {
      return {
        content: `Oops, request failed :/ - ${error}`
      }
    }

    console.log(JSON.stringify(data, null, 2));

    const response = {
      content: `<@${user.id}>`,
      embeds: buildEmbeds({
        name: data.info.name,
        region: data.info.region,
        schedule: data.schedule.days.find(x => x.name.toLowerCase() == day),
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

async function getResultsFromEspApi(id) {
  const config = {
    headers: {
      Token: process.env.ESP_AUTH_TOKEN
    }
  }

  try {
    const result = await axios.get(`https://developer.sepush.co.za/business/2.0/area?id=${id}`, config);

    return {
      success: true,
      data: result.data
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
    title: `Stage schedule for ${data.name}`,
    description: `Date: ${data.schedule.name}, ${data.schedule.date}\nRegion: ${data.region}`,
    // url: "http://example.com",
    timestamp: new Date().toISOString(),
    color: colours.Green,
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

  for (let stageIndex = 0; stageIndex < data.schedule.stages.length; stageIndex++) {
    const stage = data.schedule.stages[stageIndex];
    const stageHasSlots = stage.length > 0;
    let value = "None";

    if (stageHasSlots) {
      value = stage.join("\n").replace(/-/g, " - ")
    }

    embed.fields.push({
      name: `Stage ${stageIndex + 1}`,
      value: value,
      inline: stageIndex % 2 == 0
    });
  }

  return [embed];
}

exports.handler = (event) => {
  globalHandler(event, action)
}
