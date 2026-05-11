namespace DTO
{
    public class LoginRequestDTO
    {
        public string ten_dang_nhap_hoac_email { get; set; }
        public string mat_khau { get; set; }
    }

    public class RegisterRequestDTO
    {
        public string ten_dang_nhap { get; set; }
        public string email { get; set; }
        public string mat_khau { get; set; }
        public string xac_nhan_mat_khau { get; set; }
    }

    public class UpdateProfileRequestDTO
    {
        public string ten_dang_nhap { get; set; }
        public string email { get; set; }
        public string avatar_url { get; set; }
        public string bio { get; set; }
    }

    public class ChangePasswordRequestDTO
    {
        public string mat_khau_cu { get; set; }
        public string mat_khau_moi { get; set; }
        public string xac_nhan_mat_khau_moi { get; set; }
    }

    public class ForgotPasswordRequestDTO
    {
        public string email { get; set; }
        public string mat_khau_moi { get; set; }
        public string xac_nhan_mat_khau_moi { get; set; }
    }
}
