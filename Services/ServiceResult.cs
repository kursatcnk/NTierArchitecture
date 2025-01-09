using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace App.Services
{
    public class ServiceResult<T>
    {
        // Servisten dönen başarılı sonucu temsil eden veri.
        // Bu değeri set edilebilir yaptım, çünkü gerektiğinde dışarıdan müdahale edilmesi gerekebilir.
        public T? Data { get; set; }

        // Eğer işlem sırasında hatalar oluşursa, bunları bir liste olarak burada tutuyorum.
        // Hata durumunda birden fazla mesajın gelebileceğini göz önünde bulundurarak liste yapısını kullandım.
        public List<string>? ErrorMessage { get; set; }

        // İşlemin başarılı olup olmadığını kontrol eden bir property. 
        // Eğer hata mesajı yoksa veya hata listesi boşsa, bu işlem başarılıdır.
        [JsonIgnore]
        public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;

        // İşlemin başarısız olduğunu anlamak için ters bir kontrol yapıyorum.
        // Böylece daha okunaklı bir şekilde başarısız durumları kontrol edebiliyorum.
        [JsonIgnore]
        public bool IsFail => !IsSuccess;

        // HTTP durum kodlarını da saklayarak, servisin sonucunun HTTP seviyesinde durumunu da belirtiyorum.
        [JsonIgnore]
        public HttpStatusCode Status { get; set; }

        [JsonIgnore] public string? UrlAsCreated { get; set; }



        // Başarılı bir sonuç döndüren statik bir metot oluşturdum.
        // Bu metot, başarı durumunda data ve opsiyonel olarak bir HTTP durum kodu döndürüyor.
        public static ServiceResult<T> Success(T data, HttpStatusCode status = HttpStatusCode.OK)
        {
            return new ServiceResult<T>()
            {
                Data = data,
                Status = status
            };
        }

        public static ServiceResult<T> SuccessAsCreated(T data, string urlAsCreated)
        {
            return new ServiceResult<T>()
            {
                Data = data,
                Status = HttpStatusCode.Created,
                UrlAsCreated= urlAsCreated
            };
        }





        // Birden fazla hata mesajını döndürmek için bir statik metot ekledim.
        // Bu metot hata mesajlarını liste olarak alıyor ve varsayılan olarak "BadRequest" durum kodu döndürüyor.
        public static ServiceResult<T> Fail(List<string> errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>()
            {
                ErrorMessage = errorMessage,
                Status = status
            };
        }

        // Tek bir hata mesajını döndürmek için bir overload metot yazdım.
        // Liste yerine direkt bir string alıyor ve bunu bir listeye çeviriyor.
        public static ServiceResult<T> Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>()
            {
                ErrorMessage = new List<string> { errorMessage },
                Status = status
            };
        }
    }
    public class ServiceResult
    {
        public List<string>? ErrorMessage { get; set; }

        [JsonIgnore]
        public bool IsSuccess => ErrorMessage == null || ErrorMessage.Count == 0;
        [JsonIgnore]
        public bool IsFail => !IsSuccess;
        [JsonIgnore]
        public HttpStatusCode Status { get; set; }

        public static ServiceResult Success(HttpStatusCode status = HttpStatusCode.OK)
        {
            return new ServiceResult()
            {

                Status = status
            };
        }
        public static ServiceResult Fail(List<string> errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            return new ServiceResult()
            {
                ErrorMessage = errorMessage,
                Status = status
            };
        }
        public static ServiceResult Fail(string errorMessage, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            return new ServiceResult()
            {
                ErrorMessage = new List<string> { errorMessage },
                Status = status
            };
        }
    }
}
