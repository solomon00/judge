namespace Solomon.WebUI.Migrations
{
    using Solomon.Domain.Entities;
    using Solomon.TypesExtensions;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Solomon.Domain.Concrete.EFDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Solomon.Domain.Concrete.EFDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //List<ProgrammingLanguage> defaultLanguages = new List<ProgrammingLanguage>();

            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.C, Title = "GNU C" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.CPP, Title = "GNU C++" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.CS, Title = "C# .NET" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.VB, Title = "VB .NET" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.Java, Title = "Java" });
            //defaultLanguages.Add(new ProgrammingLanguage() { ProgrammingLanguageID = TypesExtensions.ProgrammingLanguages.Pascal, Title = "Free Pascal" });

            //foreach (ProgrammingLanguage std in defaultLanguages)
            //{
            //    if (context.ProgrammingLanguages.Count() == 0 || 
            //        context.ProgrammingLanguages.FirstOrDefault(pl => pl.ProgrammingLanguageID == std.ProgrammingLanguageID) == null)
            //            context.ProgrammingLanguages.Add(std);
            //}

            if (context.ProgrammingLanguages.Count() == 0)
            {
                foreach (ProgrammingLanguages pl in (ProgrammingLanguages[])Enum.GetValues(typeof(ProgrammingLanguages)))
                {
                    context.ProgrammingLanguages.Add(new ProgrammingLanguage()
                    {
                        ProgrammingLanguageID = pl,
                        Title = pl.ToString()
                    });
                }
            }

            if (context.ProblemTypes.Count() == 0)
            {
                foreach (ProblemTypes pt in (ProblemTypes[])Enum.GetValues(typeof(ProblemTypes)))
                {
                    context.ProblemTypes.Add(new ProblemType()
                    {
                        ProblemTypeID = pt,
                        Name = pt.ToString()
                    });
                }
            }

            if (context.TestResults.Count() == 0)
            {
                foreach (TestResults tr in (TestResults[])Enum.GetValues(typeof(TestResults)))
                {
                    context.TestResults.Add(new Solomon.Domain.Entities.TestResult()
                    {
                        TestResultID = tr,
                        Name = tr.ToString()
                    });
                }
            }

            if (context.TournamentFormats.Count() == 0)
            {
                foreach (TournamentFormats tf in (TournamentFormats[])Enum.GetValues(typeof(TournamentFormats)))
                {
                    context.TournamentFormats.Add(new TournamentFormat()
                    {
                        TournamentFormatID = tf,
                        Name = tf.ToString()
                    });
                }
            }

            if (context.TournamentTypes.Count() == 0)
            {
                foreach (TournamentTypes tt in (TournamentTypes[])Enum.GetValues(typeof(TournamentTypes)))
                {
                    context.TournamentTypes.Add(new TournamentType()
                    {
                        TournamentTypeID = tt,
                        Name = tt.ToString()
                    });
                }
            }

            if (context.UserCategories.Count() == 0)
            {
                foreach (UserCategories uc in (UserCategories[])Enum.GetValues(typeof(UserCategories)))
                {
                    context.UserCategories.Add(new Solomon.Domain.Entities.UserCategory()
                    {
                        UserCategoryID = uc,
                        Name = uc.ToString()
                    });
                }
            }

            List<string> problemTags = new List<string>()
                {
                    "Вычисления",
                    "Условия",
                    "Циклы",
                    "Массивы",
                    "Двухмерные массивы",
                    "Процедуры и функции",
                    "Работа с текстовыми файлами: ввод из файла, вывод в файл",
                    "Рекурсия",
                    "Логика",
                    "Алгоритм",
                    "Алгоритм Евклида вычисления НОД двух чисел",
                    "Проверка: является ли данное число простым методом перебора делителей",
                    "Сортировка массива пузырьком",
                    "Сортировка подсчетом",
                    "Сортировка массива: быстрая сортировка",
                    "Сортировка массива: сортировка с помощью кучи",
                    "Структуры данных: списки, хранение списка в массиве",
                    "Очереди: хранение, операции добавления и извлечения элементов",
                    "Стеки: хранение, операции добавления и извлечения элементов",
                    "Деки",
                    "Обход в ширину, поиск кратчайших расстояний в невзвешенном графе",
                    "Обход в глубину",
                    "Выделение компонент связности",
                    "Выделение мостов, точек сочленения, компонент реберной и вершинной двусвязности",
                    "Топологическая сортировка",
                    "Топологическая сортировка за O(N)",
                    "Выделение компонент сильной связности, конденсация графа",
                    "Алгоритм Дейкстры",
                    "Алгоритм Флойда",
                    "Алгоритм Форда-Беллмана",
                    "Алгоритм Кормена",
                    "Алгоритм Прима",
                    "Алгоритм Краскала",
                    "Построение эйлерова цикла в графе",
                    "Длинное сложение, вычитание",
                    "Длинное умножение",
                    "Длинное деление и извлечение корня",
                    "Алгоритм Карацубы",
                    "Вычисление чисел Cnk.",
                    "Перебор всех подмножеств данного множества",
                    "Быстрый перебор подмножеств заданной мощности данного множества",
                    "Быстрая генерация i-ой в лексикографическом порядке перестановки из N элементов",
                    "Быстрая генерация i-ой в лексикографическом порядке правильной скобочной последовательности из N пар скобок",
                    "Скалярное, векторное, смешанное произведения векторов",
                    "Нахождение площади многоугольника",
                    "Расстояние от точки до прямой",
                    "Нахождение точки пересечения двух прямых",
                    "Проверка пересечения отрезков",
                    "Нахождение выпуклой оболочки",
                    "Динамическое программирование: задача о рюкзаке",
                    "Динамическое программирование: наибольшая возрастающая подпоследовательность",
                    "Динамическое программирование: общие принципы",
                    "Метод рекурсивного спуска",
                    "Польская инверсная запись, алгоритм построение по выражению",
                    "Конечные автоматы, регулярные выражения",
                    "Контекстно-свободные грамматики, проверка принадлежности слова КС-языку",
                    "Коды Хаффмана",
                    "Алгоритм Кнута-Морриса-Пратта",
                    "Бор. Алгоритм Ахо-Корасик",
                    "Ab-отсечение, перебор с возвратом",
                    "Функция Гранди",
                    "Бинарные деревья, хранение в массиве",
                    "AVL-деревья",
                    "RB-деревья",
                    "Декартовы деревья",
                    "Нахождение наименьшего общего предка в дереве",
                    "Алгоритм Джонсона",
                    "Построение гамильтонова цикла в графе",
                    "Построение максимального паросочетания в двудольном невзвешенном графе",
                    "Венгерский алгоритм решения задачи о назначениях",
                    "Поиск максимального потока",
                    "Матрицы: определитель, обратная матрица, матричное произведение",
                    "Метод Гаусса решения систем уравнений",
                    "Дискретное преобразование Фурье",
                    "Дерево интервалов и его реализация",
                    "Динамическое дерево Тарьяна-Слейтора",
                    "Хэш-таблицы",
                    "Системы непересекающихся множеств",
                    "Обобщенный алгоритм Евклида, решение диофантовых уравнений",
                    "Метод шифрования RSA",
                    "Суффиксное дерево. Алгоритм Укконена.",
                    "Суффиксный массив. Построение без суффиксного дерева",
                    "Преобразование Бэрроуза-Уилера"
                };

            if (context.ProblemTags.Count() == 0)
            {
                foreach (var tag in problemTags)
                {
                    context.ProblemTags.Add(new ProblemTag() { Name = tag });
                }
            }

            context.SaveChanges();
        }
    }
}
