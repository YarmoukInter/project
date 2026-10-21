# دليل المطورين: استخدام البنية الجاهزة

هذا الدليل يوضح كيف تضيفون ميزة جديدة وتستفيدون من تسجيل الدخول والجلسة والموديلات والخدمات الموجودة. ابدأوا بقراءة [TEAM_START_HERE.md](TEAM_START_HERE.md) لمعرفة الصفحات التي ما زالت قوالب أولية، ثم ارجعوا لهذا الدليل عند ربط صفحة ببيانات Firebase.

## قبل البدء: ما معنى `using` وماذا أضيف أعلى الملف؟

تعليمة `using` تضاف في أعلى **ملف C# فقط** (`.cs`) لتسمح باستخدام أسماء namespaces دون كتابة المسار الكامل كل مرة. كل ملف C# يحتاج التعليميات التي يستعملها بنفسه؛ إضافتها في ملف لا يجعلها متاحة تلقائيًا في بقية الملفات. لا تضيفوا `using` في ملفات ASPX أو CSS أو JavaScript.

| إذا استخدم الملف | أضيفوا في أعلاه |
|---|---|
| `EventArgs`, `DateTime`, `Guid`, `ArgumentException` | `using System;` |
| `Task`, `Task<T>` | `using System.Threading.Tasks;` |
| `Page`, `PageAsyncTask` | `using System.Web.UI;` |
| `AuthGuard`, `AuthPage`, `AuthPolicy`, `DatabasePaths`, `FirebaseHelper` | `using Yarmouk.Helpers;` |
| `AuthSession`, `StudentService`, `ApplicationService` وغيرها | `using Yarmouk.Services;` |
| `Student`, `Application`, `Opportunity`, `User` وغيرها | `using Yarmouk.Models;` |
| `List<T>`, `Dictionary<TKey,TValue>` | `using System.Collections.Generic;` |

مثال: إذا كان ملف الصفحة يستخدم `PageAsyncTask` و`AuthGuard` و`AuthSession` و`StudentService`، يبدأ هكذا:

```csharp
using System;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;
```

## 1. تدفق الطلب داخل الموقع

عند فتح صفحة، يمر الطلب عادة بهذا المسار:

```text
المتصفح
  → صفحة ASPX (العناصر والنموذج)
  → كود الصفحة ASPX.cs (الأحداث والتحقق)
  → Service (عملية تخص المجال)
  → FirebaseHelper (اتصال HTTP بقاعدة البيانات)
  → Firebase Realtime Database
  → Model (تحويل JSON إلى كائن C#)
  → عرض النتيجة في الصفحة
```

ملف `.aspx` للهيكل، و`.aspx.cs` لمنطق الخادم، و`.aspx.designer.cs` لتعريف عناصر ASP.NET، وCSS/JavaScript منفصلان للعرض والتفاعل في المتصفح. اربطوا حدث الزر من ASPX مثل `OnClick="Save_Click"` بدالة في ملف C#.

## 2. كيف أعرف أن المستخدم مسجل دخوله؟

الحارس المركزي يعمل تلقائيًا من [Global.asax.cs](Global.asax.cs)، ويسجل `AuthGuard.CheckAsync` بعد تحميل حالة الجلسة. الصفحات الموجودة تحت `StudentPages`, `CompanyPages`, `AcademicPages`, و`CoordinatorPages` تفحص الجلسة والدور تلقائيًا في كل طلب. إذا لم توجد جلسة، يعيد المستخدم إلى صفحة الدخول. إذا كان الدور لا يطابق القسم، يوجهه الحارس إلى الصفحة المناسبة أو يرفضه.

داخل كود الصفحة، يمكن قراءة الجلسة هكذا:

```csharp
using Yarmouk.Helpers;
using Yarmouk.Services;

// داخل كلاس الصفحة:
var auth = Session[AuthGuard.SessionKey] as AuthSession;
if (auth == null)
{
    Go("~/PublicPages/Login.aspx");
    return;
}

string uid = auth.Uid;
string role = auth.User.Role;
```

`AuthSession` يحتوي UID والبريد ورمز Firebase ووقت انتهاء الجلسة وملف `User`. لا تعرضوا `IdToken` للمستخدم ولا تضعوه في JavaScript أو السجلات. في صفحات الأقسام، وجود الجلسة يُفحص مسبقًا بواسطة الحارس، لكن التحقق أعلاه مفيد عند كتابة أمثلة مستقلة أو كود يعاد استخدامه.

**مهم:** `AuthPage` يوفر الدالة `Go(path)` لإعادة التوجيه فقط؛ لا يقوم وحده بفحص تسجيل الدخول. الفحص المركزي هو `AuthGuard` المسجل من `Global.asax.cs`.

### هل أكرر فحص الجلسة والدور في كل صفحة؟

لا. الصفحات داخل `StudentPages`, `CompanyPages`, `AcademicPages`, و`CoordinatorPages` يفحصها `AuthGuard` تلقائيًا قبل تنفيذ الصفحة؛ وهو يتحقق من وجود جلسة صالحة ومن أن دور الحساب يطابق القسم. لذلك لا تكرروا فحص الصلاحيات في كل `Page_Load`.

اقرأوا `AuthSession` داخل الصفحة فقط عندما تحتاجون بيانات المستخدم لتنفيذ الميزة، مثل UID لجلب ملفه من Service. مثال القراءة موجود في القسم أعلاه. إذا أضفتم صفحة خارج مجلدات الأقسام، فلن تشملها القاعدة الحالية تلقائيًا حتى تضيفوا مسارها في `AuthPolicy.RequiredRole`. وتذكروا أن الوراثة من `AuthPage` وحدها لا تحمي الصفحة؛ هي توفر `Go(...)` لإعادة التوجيه فقط.

## 3. كيف أضيف صفحة جديدة محمية؟

1. أنشئوا الصفحة داخل مجلد الدور الصحيح، مثل `StudentPages/Attendance.aspx`.
2. استخدموا Visual Studio لإضافة Web Form حتى ينشئ ملفات ASPX وCodeBehind وDesigner ويسجلها في ملف المشروع. يجب أن يطابق `Inherits` في توجيه ASPX الـ namespace واسم كلاس CodeBehind.
3. اجعلوا كلاس الصفحة يرث `AuthPage` إذا احتجتم `Go(...)`، أو `Page` إذا لم تحتاجوها. الحماية تأتي من المسار و`AuthGuard`.
4. أضيفوا CSS وJavaScript مستقلين في `Content/<Area>` و`Scripts/<Area>` عند الحاجة.
5. إذا وضعتم الصفحة خارج مجلدات الأقسام المعروفة، أضيفوا مسارها ودورها في `AuthPolicy.RequiredRole`؛ وإلا فلن يعتبرها الحارس صفحة محمية تلقائيًا.

لا تضيفوا فحص Session/Role يدويًا إلى كل صفحة جديدة داخل مجلد الدور؛ الحارس المركزي يقوم به. أضيفوا الفحص اليدوي فقط إذا كنتم تقرأون بيانات الجلسة لاستخدامها، أو إذا كان لديكم سبب خاص لا تغطيه قاعدة الحماية المركزية.

ملف المشروع الحالي يستخدم تسجيلًا صريحًا للملفات. إذا لم يسجل Visual Studio الملفات تلقائيًا، أضيفوا العناصر المناسبة داخل `Yarmouk.csproj`:

```xml
<Content Include="StudentPages\Attendance.aspx" />
<Content Include="Content\StudentPages\Attendance.css" />
<Content Include="Scripts\StudentPages\Attendance.js" />
<Compile Include="StudentPages\Attendance.aspx.cs">
  <DependentUpon>Attendance.aspx</DependentUpon>
</Compile>
<Compile Include="StudentPages\Attendance.aspx.designer.cs">
  <DependentUpon>Attendance.aspx</DependentUpon>
</Compile>
```

مثال تحميل بيانات الطالب بشكل غير متزامن من صفحة `StudentPages`:

```csharp
using System;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Attendance : AuthPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null)
                {
                    Go("~/PublicPages/Login.aspx");
                    return;
                }

                var student = await new StudentService()
                    .GetStudentAsync(auth.Uid, auth.IdToken);

                // اربط student بعناصر الصفحة هنا.
            }));
        }
    }
}
```

ويكون توجيه الصفحة في `Attendance.aspx` مثل:

```aspx
<%@ Page Language="C#" Async="true" AutoEventWireup="true"
    CodeBehind="Attendance.aspx.cs" Inherits="Yarmouk.StudentPages.Attendance" %>
```

يجب أن يحتوي توجيه الصفحة `Async="true"` عند استخدام `RegisterAsyncTask`. استخدموا `RegisterAsyncTask` بدل `async void` في أحداث Web Forms.

## 4. الموديلات Models

الموديل يصف شكل سجل قاعدة البيانات في C#. أمثلة حالية: `User`, `Student`, `Opportunity`, و`Application` داخل `Models`.

إذا كان سجل Firebase يحتوي:

```json
{ "fullName": "Example Student", "mustChangePassword": false }
```

فالموديل المقابل يستخدم خصائص C# مثل `FullName` و`MustChangePassword`. يقوم `FirebaseHelper` بتحويل أسماء الخصائص إلى camelCase عند الكتابة، ويحوّل JSON إلى الموديل عند القراءة. أضيفوا للموديل الخصائص التي يحتاجها التدفق فقط، وتأكدوا أن أنواع البيانات تطابق JSON (مثل `bool`, `int`, `string`, أو `List<string>`).

مثال موديل صغير جديد، مثل سجل حضور:

```csharp
using System;

namespace Yarmouk.Models
{
    public class AttendanceRecord
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string Date { get; set; }
        public int Hours { get; set; }
        public string Notes { get; set; }
    }
}
```

ضعوه في `Models/AttendanceRecord.cs`. إذا أضفتم نوعًا جديدًا مثل `List<string>` فأضيفوا `using System.Collections.Generic;`.

## 5. السيرفايس Services

السيرفايس ينفذ عملية تخص ميزة، ويستدعي `FirebaseHelper` بدل وضع تفاصيل HTTP داخل الصفحة. الخدمات الموجودة حاليًا:

- `FirebaseAuthService`: تسجيل الدخول، تغيير كلمة المرور، وإرسال طلب إعادة الضبط عبر Firebase Authentication.
- `StudentService`: قراءة ملف المستخدم والطالب.
- `OpportunityService`: قراءة الفرص.
- `ApplicationService` و`EvaluationService`: قراءة سجل حسب المعرّف.
- `PasswordSetupService`: إكمال علامة إعداد كلمة المرور.

بعض هذه الخدمات توفر **قراءة فقط**؛ وجود اسم Service أو صفحة لا يعني أن سير العمل كامل. راجعوا [TEAM_PAGES_CHECKLIST.md](TEAM_PAGES_CHECKLIST.md) و[TEAM_START_HERE.md](TEAM_START_HERE.md) قبل البناء على ميزة، فالصفحات الأخرى مذكورة كقوالب أولية.

عند إضافة خدمة:

```csharp
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;

namespace Yarmouk.Services
{
    public class ExampleService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();

        public Task<ExampleModel> GetAsync(string verifiedUid, string idToken)
        {
            return firebase.GetAsync<ExampleModel>(
                DatabasePaths.Students + "/" + verifiedUid + "/example",
                idToken);
        }
    }
}
```

خذوا `verifiedUid` و`idToken` من `AuthSession` في الخادم. لا تستخدموا UID جاء من TextBox أو HiddenField لتحديد سجل المستخدم الذي يحق له الوصول إليه. استخدموا الثوابت في `DatabasePaths.cs` بدل تكرار أسماء العقد النصية في صفحات كثيرة.

## 6. عمليات Firebase الجاهزة

`FirebaseHelper` يوفر هذه الدوال:

| الدالة | الاستخدام |
|---|---|
| `GetAsync<T>` | قراءة وتحويل JSON إلى Model أو مجموعة |
| `SaveAsync<T>` | `PUT`: استبدال القيمة كاملة عند المسار |
| `UpdateAsync` | `PATCH`: تحديث مفاتيح داخل كائن باستخدام كائن تغييرات |
| `DeleteAsync` | حذف المسار |

تنبيه: عند كتابة قيمة مفردة مثل `false` أو نص، استخدموا `SaveAsync` (`PUT`). استخدموا `UpdateAsync` (`PATCH`) لكائن مثل `new { status = "approved" }` لتغيير مفتاح دون استبدال بقية السجل. يجب أن تسمح قواعد Realtime Database بالعملية والمسار.

## 7. الأدوار والجلسات

`AuthPolicy.NormalizeRole` يحول أدوار بيانات التهيئة إلى أدوار التطبيق:

| دور Firebase | دور التطبيق |
|---|---|
| `student` | `student` |
| `c_hr` | `company` |
| `uni_sp` | `academic` |
| `admin` | `coordinator` |

الدور الموثوق يأتي من `users/{uid}` في Firebase، وليس من بطاقة الدور المختارة في الواجهة. اختيار الدور في صفحة الدخول يستخدم فقط للتأكد أن المستخدم اختار نوع حسابه الصحيح؛ الخادم يقارن الاختيار بملف قاعدة البيانات قبل إنشاء الجلسة.

## 8. أين أضع التعديل؟

- عنصر أو نموذج جديد: ملف الصفحة `.aspx`، وتعريفه في `.designer.cs` (يفضل أن ينشئه Visual Studio).
- تحقق أو event handler: الصفحة المقابلة `.aspx.cs`.
- بيانات جديدة من Firebase: Model داخل `Models`.
- عملية قراءة/كتابة لميزة: Service داخل `Services`.
- مسار قاعدة بيانات مشترك أو سياسة/أداة عامة: `Helpers`، أو ثابت جديد في `DatabasePaths.cs`.
- مظهر الصفحة: CSS الخاص بها تحت `Content`.
- تفاعل المتصفح فقط: JavaScript الخاص بها تحت `Scripts`. لا تضعوا مفاتيح سرية أو منطق صلاحيات في JavaScript.

بعد الإضافة، تحققوا أن الملفات مسجلة في `Yarmouk.csproj` إذا لم يضفها Visual Studio، ثم ابنوا `Yarmouk.slnx` من Visual Studio. قبل كتابة بيانات حية، راجعوا شكل السجل وقواعد Realtime Database مع الفريق.

## 9. مثال تطبيقي لميزة ما زالت غير مكتملة: تقديم طلب تدريب

المشروع يحتوي موديل `Models/Application.cs` وخدمة `ApplicationService` للقراءة بمعرّف، لكن إنشاء طلب تقديم ليس منفذًا بعد. المثال التالي يوضح أين تضاف القطع، وليس ميزة جاهزة للاستخدام قبل إضافة التحقق من الأهلية والتكرار وقواعد Firebase.

### أ. إضافة عملية إنشاء الطلب إلى `Services/ApplicationService.cs`

```csharp
using System;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;

namespace Yarmouk.Services
{
    public class ApplicationService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();

        public Task<Application> GetByIdAsync(string id, string idToken)
        {
            return firebase.GetAsync<Application>(
                DatabasePaths.Applications + "/" + id, idToken);
        }

        public async Task<Application> SubmitAsync(
            string verifiedStudentUid, string opportunityId, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedStudentUid))
                throw new ArgumentException("Student UID is required.");
            if (string.IsNullOrWhiteSpace(opportunityId))
                throw new ArgumentException("Opportunity is required.");

            var application = new Application
            {
                Id = Guid.NewGuid().ToString("N"),
                StudentUid = verifiedStudentUid,
                OpportunityId = opportunityId,
                Status = "pending",
                AppliedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await firebase.SaveAsync(
                DatabasePaths.Applications + "/" + application.Id,
                application, idToken).ConfigureAwait(false);
            return application;
        }
    }
}
```

المعرّف الطالب يأتي من الجلسة على الخادم، أما `opportunityId` فيأتي من اختيار المستخدم ويجب التحقق من وجود الفرصة ونشاطها وأهلية الطالب قبل الحفظ. لا تعتبروا هذا المثال كاملًا قبل إضافة منع الطلبات المكررة، والتحقق من المقاعد/التخصص، ومراجعة قواعد RTDB.

### ب. استدعاء الخدمة من حدث زر الصفحة `StudentPages/Apply.aspx.cs`

أضيفوا إلى ASPX زرًا مربوطًا بـ `OnClick="Apply_Click"` وعنصرًا يحمل معرّف الفرصة المختارة. وفي ملف C#:

```csharp
using System;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Apply : AuthPage
    {
        protected void Apply_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null)
                {
                    Go("~/PublicPages/Login.aspx");
                    return;
                }

                try
                {
                    // OpportunityId هو عنصر خادم يملؤه اختيار الفرصة في الصفحة.
                    var result = await new ApplicationService().SubmitAsync(
                        auth.Uid, OpportunityId.Value, auth.IdToken);
                    Message.Text = "Application submitted.";
                }
                catch (ArgumentException)
                {
                    Message.Text = "Choose a valid opportunity.";
                }
                catch (Exception)
                {
                    Message.Text = "The application could not be saved. Please try again.";
                }
            }));
        }
    }
}
```

قبل نسخ المثال، تأكدوا أن `Apply.aspx` يحتوي عنصرًا خادميًا باسم `OpportunityId` وعنصر `Message`، وأن توجيه الصفحة فيه `Async="true"`. لا تعرضوا تفاصيل الاستثناء أو `IdToken` للمستخدم. أضيفوا تحققًا من `OpportunityId` في السيرفر؛ لا تثقوا بقيمة HiddenField وحدها.
