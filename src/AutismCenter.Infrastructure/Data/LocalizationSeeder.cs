using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutismCenter.Infrastructure.Data;

public static class LocalizationSeeder
{
    public static async Task SeedLocalizationDataAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (await context.LocalizedContents.AnyAsync() || await context.EmailTemplates.AnyAsync())
        {
            return; // Data already seeded
        }

        // Seed Localized Content
        var localizedContents = new List<LocalizedContent>
        {
            // UI Messages - English
            new LocalizedContent("welcome_message", Language.English, "Welcome to Autism Center", "ui", "Main welcome message", "system"),
            new LocalizedContent("login_required", Language.English, "Please log in to access this feature", "ui", "Login required message", "system"),
            new LocalizedContent("access_denied", Language.English, "Access denied. You don't have permission to view this page.", "ui", "Access denied message", "system"),
            new LocalizedContent("loading", Language.English, "Loading...", "ui", "Loading indicator text", "system"),
            new LocalizedContent("save_success", Language.English, "Changes saved successfully", "ui", "Success message for save operations", "system"),
            new LocalizedContent("save_error", Language.English, "An error occurred while saving changes", "ui", "Error message for save operations", "system"),
            new LocalizedContent("confirm_delete", Language.English, "Are you sure you want to delete this item?", "ui", "Delete confirmation message", "system"),
            new LocalizedContent("item_deleted", Language.English, "Item deleted successfully", "ui", "Delete success message", "system"),
            new LocalizedContent("no_data_found", Language.English, "No data found", "ui", "No data message", "system"),
            new LocalizedContent("search_placeholder", Language.English, "Search...", "ui", "Search input placeholder", "system"),

            // UI Messages - Arabic
            new LocalizedContent("welcome_message", Language.Arabic, "مرحباً بكم في مركز التوحد", "ui", "رسالة الترحيب الرئيسية", "system"),
            new LocalizedContent("login_required", Language.Arabic, "يرجى تسجيل الدخول للوصول إلى هذه الميزة", "ui", "رسالة تسجيل الدخول المطلوبة", "system"),
            new LocalizedContent("access_denied", Language.Arabic, "تم رفض الوصول. ليس لديك إذن لعرض هذه الصفحة.", "ui", "رسالة رفض الوصول", "system"),
            new LocalizedContent("loading", Language.Arabic, "جاري التحميل...", "ui", "نص مؤشر التحميل", "system"),
            new LocalizedContent("save_success", Language.Arabic, "تم حفظ التغييرات بنجاح", "ui", "رسالة نجاح عمليات الحفظ", "system"),
            new LocalizedContent("save_error", Language.Arabic, "حدث خطأ أثناء حفظ التغييرات", "ui", "رسالة خطأ عمليات الحفظ", "system"),
            new LocalizedContent("confirm_delete", Language.Arabic, "هل أنت متأكد من أنك تريد حذف هذا العنصر؟", "ui", "رسالة تأكيد الحذف", "system"),
            new LocalizedContent("item_deleted", Language.Arabic, "تم حذف العنصر بنجاح", "ui", "رسالة نجاح الحذف", "system"),
            new LocalizedContent("no_data_found", Language.Arabic, "لم يتم العثور على بيانات", "ui", "رسالة عدم وجود بيانات", "system"),
            new LocalizedContent("search_placeholder", Language.Arabic, "بحث...", "ui", "نص البحث التوضيحي", "system"),

            // Navigation - English
            new LocalizedContent("nav_home", Language.English, "Home", "navigation", "Home navigation link", "system"),
            new LocalizedContent("nav_products", Language.English, "Products", "navigation", "Products navigation link", "system"),
            new LocalizedContent("nav_courses", Language.English, "Courses", "navigation", "Courses navigation link", "system"),
            new LocalizedContent("nav_appointments", Language.English, "Appointments", "navigation", "Appointments navigation link", "system"),
            new LocalizedContent("nav_about", Language.English, "About Us", "navigation", "About Us navigation link", "system"),
            new LocalizedContent("nav_contact", Language.English, "Contact", "navigation", "Contact navigation link", "system"),
            new LocalizedContent("nav_login", Language.English, "Login", "navigation", "Login navigation link", "system"),
            new LocalizedContent("nav_register", Language.English, "Register", "navigation", "Register navigation link", "system"),
            new LocalizedContent("nav_profile", Language.English, "Profile", "navigation", "Profile navigation link", "system"),
            new LocalizedContent("nav_logout", Language.English, "Logout", "navigation", "Logout navigation link", "system"),

            // Navigation - Arabic
            new LocalizedContent("nav_home", Language.Arabic, "الرئيسية", "navigation", "رابط التنقل للصفحة الرئيسية", "system"),
            new LocalizedContent("nav_products", Language.Arabic, "المنتجات", "navigation", "رابط التنقل للمنتجات", "system"),
            new LocalizedContent("nav_courses", Language.Arabic, "الدورات", "navigation", "رابط التنقل للدورات", "system"),
            new LocalizedContent("nav_appointments", Language.Arabic, "المواعيد", "navigation", "رابط التنقل للمواعيد", "system"),
            new LocalizedContent("nav_about", Language.Arabic, "من نحن", "navigation", "رابط التنقل لصفحة من نحن", "system"),
            new LocalizedContent("nav_contact", Language.Arabic, "اتصل بنا", "navigation", "رابط التنقل لصفحة الاتصال", "system"),
            new LocalizedContent("nav_login", Language.Arabic, "تسجيل الدخول", "navigation", "رابط التنقل لتسجيل الدخول", "system"),
            new LocalizedContent("nav_register", Language.Arabic, "إنشاء حساب", "navigation", "رابط التنقل لإنشاء حساب", "system"),
            new LocalizedContent("nav_profile", Language.Arabic, "الملف الشخصي", "navigation", "رابط التنقل للملف الشخصي", "system"),
            new LocalizedContent("nav_logout", Language.Arabic, "تسجيل الخروج", "navigation", "رابط التنقل لتسجيل الخروج", "system"),

            // Error Messages - English
            new LocalizedContent("error_general", Language.English, "An unexpected error occurred. Please try again.", "error", "General error message", "system"),
            new LocalizedContent("error_network", Language.English, "Network error. Please check your connection.", "error", "Network error message", "system"),
            new LocalizedContent("error_validation", Language.English, "Please check your input and try again.", "error", "Validation error message", "system"),
            new LocalizedContent("error_unauthorized", Language.English, "You are not authorized to perform this action.", "error", "Unauthorized error message", "system"),
            new LocalizedContent("error_not_found", Language.English, "The requested resource was not found.", "error", "Not found error message", "system"),

            // Error Messages - Arabic
            new LocalizedContent("error_general", Language.Arabic, "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.", "error", "رسالة خطأ عامة", "system"),
            new LocalizedContent("error_network", Language.Arabic, "خطأ في الشبكة. يرجى التحقق من اتصالك.", "error", "رسالة خطأ الشبكة", "system"),
            new LocalizedContent("error_validation", Language.Arabic, "يرجى التحقق من المدخلات والمحاولة مرة أخرى.", "error", "رسالة خطأ التحقق", "system"),
            new LocalizedContent("error_unauthorized", Language.Arabic, "غير مخول لك تنفيذ هذا الإجراء.", "error", "رسالة خطأ عدم التخويل", "system"),
            new LocalizedContent("error_not_found", Language.Arabic, "لم يتم العثور على المورد المطلوب.", "error", "رسالة خطأ عدم العثور", "system")
        };

        await context.LocalizedContents.AddRangeAsync(localizedContents);

        // Seed Email Templates
        var emailTemplates = new List<EmailTemplate>
        {
            // Email Verification Templates
            new EmailTemplate("email_verification", Language.English, 
                "Verify Your Email Address", 
                @"Hello {{firstName}},

Thank you for registering with Autism Center. To complete your registration, please verify your email address by clicking the link below:

{{verificationUrl}}

If you did not create an account with us, please ignore this email.

Best regards,
Autism Center Team", 
                "Email verification template in English", "system"),

            new EmailTemplate("email_verification", Language.Arabic, 
                "تأكيد عنوان بريدك الإلكتروني", 
                @"مرحباً {{firstName}},

شكراً لك على التسجيل في مركز التوحد. لإكمال تسجيلك، يرجى تأكيد عنوان بريدك الإلكتروني بالنقر على الرابط أدناه:

{{verificationUrl}}

إذا لم تقم بإنشاء حساب معنا، يرجى تجاهل هذا البريد الإلكتروني.

مع أطيب التحيات،
فريق مركز التوحد", 
                "قالب تأكيد البريد الإلكتروني باللغة العربية", "system"),

            // Welcome Email Templates
            new EmailTemplate("welcome_email", Language.English, 
                "Welcome to Autism Center", 
                @"Hello {{firstName}},

Welcome to Autism Center! Your account has been successfully verified and you now have access to all our services.

You can now:
- Browse and purchase our autism-related products
- Enroll in our educational courses
- Book appointments with our specialists
- Access your personalized dashboard

If you have any questions, please don't hesitate to contact us.

Best regards,
Autism Center Team", 
                "Welcome email template in English", "system"),

            new EmailTemplate("welcome_email", Language.Arabic, 
                "مرحباً بك في مركز التوحد", 
                @"مرحباً {{firstName}},

مرحباً بك في مركز التوحد! تم تأكيد حسابك بنجاح ويمكنك الآن الوصول إلى جميع خدماتنا.

يمكنك الآن:
- تصفح وشراء منتجاتنا المتعلقة بالتوحد
- التسجيل في دوراتنا التعليمية
- حجز مواعيد مع أخصائيينا
- الوصول إلى لوحة التحكم الشخصية الخاصة بك

إذا كان لديك أي أسئلة، يرجى عدم التردد في الاتصال بنا.

مع أطيب التحيات،
فريق مركز التوحد", 
                "قالب البريد الإلكتروني الترحيبي باللغة العربية", "system"),

            // Password Reset Templates
            new EmailTemplate("password_reset", Language.English, 
                "Reset Your Password", 
                @"Hello {{firstName}},

We received a request to reset your password for your Autism Center account. Click the link below to reset your password:

{{resetUrl}}

This link will expire in 24 hours for security reasons.

If you did not request a password reset, please ignore this email and your password will remain unchanged.

Best regards,
Autism Center Team", 
                "Password reset email template in English", "system"),

            new EmailTemplate("password_reset", Language.Arabic, 
                "إعادة تعيين كلمة المرور", 
                @"مرحباً {{firstName}},

تلقينا طلباً لإعادة تعيين كلمة المرور لحسابك في مركز التوحد. انقر على الرابط أدناه لإعادة تعيين كلمة المرور:

{{resetUrl}}

سينتهي صلاحية هذا الرابط خلال 24 ساعة لأسباب أمنية.

إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني وستبقى كلمة المرور الخاصة بك دون تغيير.

مع أطيب التحيات،
فريق مركز التوحد", 
                "قالب البريد الإلكتروني لإعادة تعيين كلمة المرور باللغة العربية", "system")
        };

        await context.EmailTemplates.AddRangeAsync(emailTemplates);

        await context.SaveChangesAsync();
    }
}