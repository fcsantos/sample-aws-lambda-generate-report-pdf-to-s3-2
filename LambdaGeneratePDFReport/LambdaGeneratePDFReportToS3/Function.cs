using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaGeneratePDFReportToS3;

public partial class Function
{
    private readonly IAmazonS3 _s3Client;
    private const string OUTPUT_BUCKET = "bucket-sample-lambda-pdf";

    public Function()
    {
        _s3Client = new AmazonS3Client();
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> FunctionHandler(ReportRequest request, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processando relatório para o período: {request.StartDate} até {request.EndDate}");

            var dados = await BuscarDados(request.StartDate, request.EndDate);

            var document = new VendasDocument(dados, request.StartDate, request.EndDate);
            byte[] pdfBytes = document.GeneratePdf();

            string fileName = $"relatorio_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = OUTPUT_BUCKET,
                Key = fileName,
                InputStream = new MemoryStream(pdfBytes)
            });

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = OUTPUT_BUCKET,
                Key = fileName,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            string downloadUrl = _s3Client.GetPreSignedURL(urlRequest);

            return System.Text.Json.JsonSerializer.Serialize(new
            {
                Message = "Relatório gerado com sucesso",
                DownloadUrl = downloadUrl
            });
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro ao gerar relatório: {ex.Message}");
            throw;
        }
    }

    private async Task<List<DadosVenda>> BuscarDados(DateTime startDate, DateTime endDate)
    {
        // Simula busca de dados - implemente sua lógica real aqui
        return new List<DadosVenda>
        {
            new DadosVenda { Data = DateTime.Now, Produto = "Produto A", Quantidade = 10, ValorTotal = 1000 },
            new DadosVenda { Data = DateTime.Now.AddDays(-1), Produto = "Produto B", Quantidade = 5, ValorTotal = 500 }
        };
    }
}