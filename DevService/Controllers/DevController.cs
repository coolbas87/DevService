using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DevService.Models;
using Microsoft.EntityFrameworkCore;
using DevService.RefData;
using Microsoft.Extensions.Logging;

namespace DevService.Controllers
{
    [Route("/")]
    [ApiController]
    public class DevController : ControllerBase
    {
        private readonly MainContext _context;
        private readonly ILogger _logger;

        public DevController(MainContext context, ILogger<DevController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private BadRequestObjectResult RequestNotPOSTError()
        {
            return BadRequest(new ErrorItem("Тип запиту не POST"));
        }

        // GET:
        [HttpGet]
        public ActionResult<ErrorItem> Get()
        {
            return RequestNotPOSTError();
        }

        private JArray sqlDataReaderToJsonArr(SqlDataReader dataReader)
        {
            JArray arr = new JArray();
            while (dataReader.Read())
            {
                JObject obj = new JObject();
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    obj.Add(new JProperty(dataReader.GetName(i), dataReader.GetValue(i)));
                }
                arr.Add(obj);
            }
            return arr;
        }

        private async Task<JObject> CreateResponse(string command, JObject reqParams, bool fixedParam = false)
        {
            if (command.Equals("*Refresh"))
            {
                RefBook.Refresh(_context);
                return await Task.Run(() => DoRefreshCommand());
            }
            else
                return await DoCommand(command, reqParams, fixedParam);
        }

        private JObject DoRefreshCommand()
        {
            RefBook.Refresh(_context);
            return new JObject();
        }

        private async Task<JObject> DoCommand(string command, JObject reqParams, bool fixedParam = false)
        {
            Commands commandObj = RefBook.Commands.SingleOrDefault(cmd => cmd.cName == command);

            SqlConnectionStringBuilder myBuilder = new SqlConnectionStringBuilder();
            myBuilder.IntegratedSecurity = true;
            myBuilder.InitialCatalog = commandObj.Base;
            myBuilder.DataSource = commandObj.Server;
            myBuilder.ConnectTimeout = 30;

            SqlConnection connection = new SqlConnection(myBuilder.ConnectionString);
            SqlCommand myCommand = new SqlCommand(commandObj.ProcName, connection);
            myCommand.CommandType = System.Data.CommandType.StoredProcedure;
            JObject JSONRes = new JObject();
            connection.Open();
            try
            {
                SqlCommandBuilder.DeriveParameters(myCommand);
                if (fixedParam)
                {
                    myCommand.Parameters["@Params"].Value = reqParams.ToString();
                }
                else
                {
                    foreach (JProperty reqParam in reqParams.Children<JProperty>())
                    {
                        CommandParams comParam = commandObj.Params.Single(prm => prm.pName == reqParam.Name);
                        if (myCommand.Parameters.Contains($"@{comParam.pOurName}"))
                        {
                            switch (reqParam.Value.Type)
                            {
                                case JTokenType.Integer:
                                    myCommand.Parameters[$"@{comParam.pOurName}"].Value = (int)reqParam.Value;
                                    break;
                                case JTokenType.Float:
                                    myCommand.Parameters[$"@{comParam.pOurName}"].Value = (double)reqParam.Value;
                                    break;
                                case JTokenType.Boolean:
                                    myCommand.Parameters[$"@{comParam.pOurName}"].Value = (bool)reqParam.Value;
                                    break;
                                case JTokenType.Null:
                                    myCommand.Parameters[$"@{comParam.pOurName}"].Value = DBNull.Value;
                                    break;
                                case JTokenType.Date:
                                    myCommand.Parameters[$"@{comParam.pOurName}"].Value = (DateTime)reqParam.Value;
                                    break;
                                default: // String and other
                                    myCommand.Parameters[$"@{comParam.pOurName}"].SqlValue = reqParam.Value.ToString();
                                    break;
                            }
                        }
                    }
                }
                SqlDataReader reader = await myCommand.ExecuteReaderAsync();
                try
                {
                    JSONRes.Add("Cursor", sqlDataReaderToJsonArr(reader));
                    JSONRes.Add("CommandRes", new JValue(0));
                    JSONRes.Add("CommandError", new JValue(String.Empty));
                }
                finally
                {
                    reader.Close();
                }
            }
            finally
            {
                connection.Close();
            }
            return JSONRes;
        }

        // POST:
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] JObject value)
        {
            JToken commandVal;
            if (!value.TryGetValue("Command", out commandVal))
            {
                throw new KeyNotFoundException();
            }

            string command = ((JValue)commandVal).ToString();

            JObject reqParams = (JObject)value.GetValue("Params");
            return Ok(await CreateResponse(command, reqParams));
        }

        // POST:
        [HttpPost("{command}")]
        public async Task<ActionResult> Post(string command, [FromBody] JObject value)
        {
            return Ok(await CreateResponse(command, value, true));
        }

        // PUT: 
        [HttpPut()]
        public ActionResult<ErrorItem> Put()
        {
            return RequestNotPOSTError();
        }

        // DELETE: 
        [HttpDelete()]
        public ActionResult<ErrorItem> Delete()
        {
            return RequestNotPOSTError();
        }
    }
}
