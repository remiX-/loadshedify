const axios = require('axios').default;

exports.globalHandler = async (event, action) => {
  /*
   * Should be changed to respond differently depending on interaction type.
   * Now it only edits "Loading..." message, therefore only answers to
   * text interactions.
   */
  const body = JSON.parse(event.Records[0].Sns.Message);
  const response = await action(body);

  console.log("~~~");
  console.log(JSON.stringify(body, null, 2));
  console.log(body.member.user.id);
  console.log("~~~");

  try {
    await axios.patch(`https://discord.com/api/v10/webhooks/${body.application_id}/${body.token}/messages/@original`, response);

    console.log("Great success!");
  } catch (err) {
    console.log("Something went wrong patching from SNS");
    console.log(err);

    const failedResponse = {
      content: `Internal server error: ${err}`
    }

    await axios.patch(`https://discord.com/api/v10/webhooks/${body.application_id}/${body.token}/messages/@original`, failedResponse);
  }
}
