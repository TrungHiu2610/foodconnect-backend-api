using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodConnect.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialSystemConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SystemConfigs",
                columns: new[] { "Id", "ConfigKey", "ConfigValue", "Description", "DataType", "IsEditable", "CreatedAtUtc", "CreatedBy", "IsDeleted" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "CommissionRate", "5", "Platform commission rate percentage (0-100)", "decimal", true, DateTime.UtcNow, null, false },
                    { Guid.NewGuid(), "MinWithdrawAmount", "100000", "Minimum withdrawal amount in VND", "decimal", true, DateTime.UtcNow, null, false },
                    { Guid.NewGuid(), "TermsOfService", "Điều khoản dịch vụ FoodConnect:\n\n1. Chấp nhận điều khoản\nBằng việc sử dụng nền tảng FoodConnect, bạn đồng ý tuân thủ các điều khoản và điều kiện được quy định dưới đây.\n\n2. Đăng ký tài khoản\n- Người dùng phải cung cấp thông tin chính xác khi đăng ký.\n- Mỗi người chỉ được tạo một tài khoản.\n- Bạn chịu trách nhiệm bảo mật thông tin đăng nhập.\n\n3. Quy định về bán hàng\n- Người bán phải đảm bảo thực phẩm đạt tiêu chuẩn an toàn.\n- Mô tả sản phẩm phải trung thực, không gây hiểu lầm.\n- Giá cả phải minh bạch, không được tăng giá đột ngột.\n\n4. Thanh toán và hoa hồng\n- FoodConnect thu hoa hồng mặc định 5% trên mỗi đơn hàng.\n- Thanh toán được xử lý qua VNPay.\n- Tiền sẽ được chuyển vào ví người bán sau khi đơn hàng hoàn thành.\n\n5. Rút tiền\n- Số tiền rút tối thiểu: 100,000 VNĐ.\n- Yêu cầu rút tiền sẽ được xử lý trong vòng 3-5 ngày làm việc.\n\n6. Chính sách hủy đơn\n- Người mua có thể hủy đơn trước khi người bán xác nhận.\n- Sau khi xác nhận, chỉ người bán mới có thể hủy với lý do hợp lý.\n\n7. Quyền và nghĩa vụ của FoodConnect\n- FoodConnect có quyền tạm ngưng hoặc khóa tài khoản vi phạm.\n- Nền tảng không chịu trách nhiệm về tranh chấp giữa người mua và người bán.\n\n8. Thay đổi điều khoản\nFoodConnect có quyền cập nhật điều khoản bất cứ lúc nào. Người dùng sẽ được thông báo qua email hoặc thông báo trên nền tảng.", "Terms of Service for FoodConnect platform", "text", true, DateTime.UtcNow, null, false },
                    { Guid.NewGuid(), "PrivacyPolicy", "Chính sách bảo mật FoodConnect:\n\n1. Thu thập thông tin\nChúng tôi thu thập các thông tin sau:\n- Thông tin cá nhân: Họ tên, email, số điện thoại, địa chỉ.\n- Thông tin thanh toán: Thông tin tài khoản ngân hàng (được mã hóa).\n- Thông tin sử dụng: Lịch sử mua hàng, tìm kiếm, đánh giá.\n\n2. Mục đích sử dụng thông tin\n- Xử lý đơn hàng và thanh toán.\n- Cải thiện trải nghiệm người dùng.\n- Gửi thông báo về đơn hàng, khuyến mãi (với sự đồng ý).\n- Phân tích dữ liệu để tối ưu hóa nền tảng.\n\n3. Bảo vệ thông tin\n- Thông tin được mã hóa bằng SSL/TLS.\n- Chỉ nhân viên được ủy quyền mới có thể truy cập dữ liệu.\n- Hệ thống được bảo vệ bởi tường lửa và các biện pháp bảo mật hiện đại.\n\n4. Chia sẻ thông tin với bên thứ ba\nChúng tôi chỉ chia sẻ thông tin khi:\n- Được sự đồng ý của bạn.\n- Yêu cầu pháp lý từ cơ quan có thẩm quyền.\n- Với đối tác thanh toán (VNPay) để xử lý giao dịch.\n\n5. Quyền của người dùng\nBạn có quyền:\n- Truy cập và chỉnh sửa thông tin cá nhân.\n- Yêu cầu xóa tài khoản và dữ liệu.\n- Từ chối nhận email marketing.\n\n6. Cookie và công nghệ theo dõi\n- Chúng tôi sử dụng cookie để cải thiện trải nghiệm người dùng.\n- Bạn có thể vô hiệu hóa cookie trong trình duyệt.\n\n7. Lưu trữ dữ liệu\n- Dữ liệu được lưu trữ tại các máy chủ đạt chuẩn bảo mật quốc tế.\n- Thông tin thanh toán không được lưu trữ trực tiếp trên hệ thống.\n\n8. Liên hệ\nNếu có thắc mắc về chính sách bảo mật, vui lòng liên hệ: support@foodconnect.vn", "Privacy Policy for user data protection", "text", true, DateTime.UtcNow, null, false },
                    { Guid.NewGuid(), "ReturnPolicy", "Chính sách hoàn trả FoodConnect:\n\n1. Điều kiện hoàn trả\nĐơn hàng có thể được hoàn trả trong các trường hợp sau:\n- Sản phẩm bị hỏng, hư hại khi nhận hàng.\n- Sản phẩm không đúng với mô tả.\n- Sản phẩm hết hạn sử dụng.\n- Thiếu sản phẩm trong đơn hàng.\n\n2. Thời gian yêu cầu hoàn trả\n- Người mua phải gửi yêu cầu trong vòng 24 giờ kể từ khi nhận hàng.\n- Cần cung cấp hình ảnh/video minh chứng.\n\n3. Quy trình hoàn trả\nBước 1: Người mua gửi yêu cầu hoàn trả kèm hình ảnh.\nBước 2: FoodConnect xem xét và liên hệ người bán.\nBước 3: Nếu được chấp nhận, người bán hoàn tiền hoặc gửi sản phẩm mới.\nBước 4: Tiền hoàn được chuyển vào ví người mua trong vòng 3-5 ngày.\n\n4. Trường hợp không được hoàn trả\n- Sản phẩm đã được sử dụng (trừ trường hợp lỗi chất lượng).\n- Không có bằng chứng hợp lệ.\n- Quá thời hạn 24 giờ.\n- Người mua cung cấp thông tin sai lệch.\n\n5. Chi phí hoàn trả\n- Nếu lỗi từ người bán: Chi phí do người bán chịu.\n- Nếu lỗi từ người mua: Chi phí do người mua chịu.\n- Trường hợp đặc biệt: FoodConnect sẽ là bên trung gian giải quyết.\n\n6. Hoàn tiền\n- Tiền sẽ được hoàn vào ví FoodConnect của người mua.\n- Người mua có thể rút về tài khoản ngân hàng hoặc sử dụng cho đơn hàng tiếp theo.\n\n7. Liên hệ hỗ trợ\nMọi thắc mắc về hoàn trả, vui lòng liên hệ:\n- Email: support@foodconnect.vn\n- Hotline: 1900-xxxx", "Return and Refund Policy", "text", true, DateTime.UtcNow, null, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"SystemConfigs\" WHERE \"ConfigKey\" IN ('CommissionRate', 'MinWithdrawAmount', 'TermsOfService', 'PrivacyPolicy', 'ReturnPolicy')");
        }
    }
}
