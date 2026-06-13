namespace Bookstore.AdminClient.Services
{
    using System.Net;

    public class ApiErrorHandler : DelegatingHandler
    {
        private readonly NotificationService _notification;

        public ApiErrorHandler(
            NotificationService notification)
        {
            _notification = notification;
        }

        protected override async Task<HttpResponseMessage>
            SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            try
            {
                var response =
                    await base.SendAsync(
                        request,
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:

                            _notification.Error(
                                "Dữ liệu không hợp lệ.");

                            break;

                        case HttpStatusCode.Unauthorized:

                            _notification.Error(
                                "Phiên đăng nhập đã hết hạn.");

                            break;

                        case HttpStatusCode.Forbidden:

                            _notification.Error(
                                "Bạn không có quyền truy cập.");

                            break;

                        case HttpStatusCode.NotFound:

                            _notification.Error(
                                "Không tìm thấy dữ liệu.");

                            break;

                        case HttpStatusCode.InternalServerError:

                            _notification.Error(
                                "Máy chủ đang gặp sự cố.");

                            break;

                        default:

                            _notification.Error(
                                $"Lỗi {(int)response.StatusCode}");

                            break;
                    }
                }

                return response;
            }
            catch (HttpRequestException)
            {
                _notification.Error(
                    "Không thể kết nối tới máy chủ.");

                throw;
            }
        }
    }
}
