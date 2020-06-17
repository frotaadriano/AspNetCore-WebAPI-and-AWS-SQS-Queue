using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace APIAcessoSQS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        private readonly IAmazonSQS sqs = new AmazonSQSClient(RegionEndpoint.USEast1);
        private readonly string filaName = "TarefaQueue";

        private string GetQueueURL()
        {
            return sqs.GetQueueUrlAsync(filaName).Result.QueueUrl;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string queueURL = GetQueueURL();

            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueURL
            };

            Task<ReceiveMessageResponse> receiveMessageResponse = sqs.ReceiveMessageAsync(receiveMessageRequest);

            var msgDaFila = new StringBuilder();

            foreach (Message message in receiveMessageResponse.Result.Messages)
            {
                msgDaFila.Append("= Message = \n");
                msgDaFila.Append("\nMessageId: " + message.MessageId.ToString());
                msgDaFila.Append("\nReceiptHandle:  " + message.ReceiptHandle);
                msgDaFila.Append("\nMD5OfBody: " + message.MD5OfBody);
                msgDaFila.Append("\nBody: " + message.Body);
            }
            return Ok(msgDaFila.ToString());
        }



        [HttpPost]
        public IActionResult Post([FromBody] string item)
        {
            SendMessageRequest sendMessageRequest = new SendMessageRequest();
            sendMessageRequest.QueueUrl = GetQueueURL(); //URL from initial queue creation
            sendMessageRequest.MessageBody = JsonConvert.SerializeObject(item); // "This is my message text.";

            try
            {
                sqs.SendMessageAsync(sendMessageRequest);
                return Ok();
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }



    }
}