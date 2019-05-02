using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CEK.CSharp;
using CEK.CSharp.Models;

namespace MangaClova
{
    public static class Clova
    {
        [FunctionName("Clova")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //リクエストボディのJSONを検証してC#のクラスに変換
            var clovaRequest = await new ClovaClient().GetRequest(req.Headers["SignatureCEK"], req.Body);
            //返事を作成
            var clovaResponse = new CEKResponse();
            // // log.LogInformation("clovaRequest.Request.Type　%d" , clovaRequest.Request.Type);
            switch (clovaRequest.Request.Type)
            {
                case RequestType.LaunchRequest:
                 //起動時の処理
                    clovaResponse.AddText("こんにちは。何を占ってほしいですか？");
                    clovaResponse.AddText("健康運と仕事運と金運を占えるよ。");
                    clovaResponse.AddText("例えば健康運を占ってって聞いてみてね。");
                    clovaResponse.ShouldEndSession = false; // スキルを終わらせないように設定する
                break;
                case RequestType.SessionEndedRequest:
                     // 終了時の処理。スキルが終了したときに何かしたい時は、ここに処理を追加します
                     clovaResponse.AddText ("いつでも呼んでくださいね。");
                break;
                
                case RequestType.IntentRequest:
 
                      // インテントの処理
                    switch (clovaRequest.Request.Intent.Name)
                    {
                        case "FortuneTellingIntent":
                        // 占いのインテント
                        // 占い対象がスロットに無い場合は総合運勢を占う
                        var fortune = "総合運勢";
                        if (clovaRequest.Request.Intent.Slots != null && clovaRequest.Request.Intent.Slots.TryGetValue("FortuneSlot", out var fortuneSlot))
                        {
                            // 占いの対象がある場合は、それをスロットから取得する
                             fortune = fortuneSlot.Value;
                        }

                        // 占いの結果を返す
                        var result = new[]{ "だいきち", "ちゅうきち", "しょうきち", "すえきち", "きち", "きょう", "だいきょう" }[new Random().Next(7)];
                        clovaResponse.AddText($"{fortune} は {result} です。");
                        break;
                        default:
                        // 認識できなかったインテント
                        clovaResponse.AddText("ごめんなさい。よくわかりませんでした。例えば健康運を占ってと言うと健康運を占います。");
                        clovaResponse.ShouldEndSession = false; // スキルを終わらせないように設定する
                        break;
                    }
                 break;
            }
            // レスポンスとして作成した返事の内容を返す
            return new OkObjectResult(clovaResponse);

        }
    }
}
