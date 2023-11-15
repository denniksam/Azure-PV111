namespace Azure_PV111.Models.Home.Data
{
    public record HomeDataFormModel
    {
        public String Comment { get; set; } = null!;
        public String Section { get; set; } = null!;
    }
}
/* Випробувати режим збереження даних у RAM 
 * на опублікованому сайті (на Azure), перевірити
 * як впливає на зміст даних перепублікація проєкту.
 * Надіслати посилання на сайт, додати скриншоти
 * до та після редеплою.
 */