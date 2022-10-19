package calendarclient;

import java.io.IOException;
import java.net.URISyntaxException;
import java.util.InputMismatchException;
import java.util.List;
import java.util.Scanner;
import java.util.concurrent.ExecutionException;

public class ConsoleInterface
{
    private final String[] options = new String[]{"1 - Вывести список календарей", "2 - Добавить новый календарь", "3 - Редактировать календарь", "4 - Удалить календарь", "5 - Выход"};
    private final CalendarWorker calendarWorker;
    private final List<Calendar> calendars;

    public ConsoleInterface() throws IOException, URISyntaxException, ExecutionException, InterruptedException
    {
        calendarWorker = new CalendarWorker();
        calendars = calendarWorker.getCalendars();
    }

    public void start() throws IOException, ExecutionException, InterruptedException
    {
        int option;

        while (true)
        {
            printMenu();

            option = readNumberBeforeInclusive(5);

            switch (option)
            {
                case 1:
                    writeCalendars(calendars);
                    break;
                case 2:
                    addCalendar();
                    break;
                case 3:
                    updateCalendar();
                    break;
                case 4:
                    removeCalendar();
                    break;
                case 5:
                    finishWork();
                    return;
            }
        }
    }

    private void printMenu()
    {
        System.out.println("Пожалуйста, выберите операцию:");

        for (String option : options)
        {
            System.out.println(option);
        }
    }

    private void writeCalendars(List<Calendar> calendars)
    {
        for (int i = 0; i < calendars.size(); i++)
        {
            System.out.println(i + 1 + ") " + calendars.get(i));
        }
    }

    private void addCalendar() throws IOException, ExecutionException, InterruptedException
    {
        Scanner scanner = new Scanner(System.in);

        System.out.println("Введите название добавляемого календаря:");
        String summary = scanner.next();

        System.out.println("Введите описание добавляемого календаря:");
        String description = scanner.next();

        Calendar calendar = new Calendar(summary, description);

        Boolean isAdded = calendarWorker.addCalendar(calendar);

        if (isAdded)
        {
            calendars.add(calendar);
        }

        System.out.println(isAdded ? "Календарь был успешно добавлен" : "При добавлении календаря произошла ошибка");
    }

    private void updateCalendar() throws ExecutionException, InterruptedException, IOException
    {
        Scanner scanner = new Scanner(System.in);

        System.out.println("Введите номер календаря:");
        int number = readNumberBeforeInclusive(calendars.size());

        Calendar calendar = calendars.get(number - 1);

        System.out.println("Введите новое название редактируемого календаря:");
        String summary = scanner.next();
        calendar.setSummary(summary);

        System.out.println("Введите новое описание редактируемого календаря:");
        String description = scanner.next();
        calendar.setDescription(description);

        Boolean isUpdated = calendarWorker.updateCalendar(calendar);
        System.out.println(isUpdated ? "Календарь был успешно обновлён" : "При обновлении календаря произошла ошибка");
    }

    private void removeCalendar() throws IOException, ExecutionException, InterruptedException
    {
        System.out.println("Введите номер календаря:");
        int number = readNumberBeforeInclusive(calendars.size());

        Calendar calendar = calendars.get(number - 1);

        Boolean isRemoved =  calendarWorker.removeCalendar(calendar);

        if (isRemoved)
        {
            calendars.remove(calendar);
        }

        System.out.println(isRemoved ? "Календарь был успешно удалён" : "При удалении календаря произошла ошибка");
    }

    private void finishWork()
    {
        System.out.println("Завершение работы");
    }

    private int readNumberBeforeInclusive(int upperBound)
    {
        Scanner scanner = new Scanner(System.in);
        int number = 0;

        while (number == 0 || number > upperBound)
        {
            try
            {
                number = scanner.nextInt();
            }
            catch (InputMismatchException exception)
            {
                System.out.println("Пожалуйста, введите целое число от " + 1 + " до " + upperBound + " включительно:");
            }

            if (number < 1 || number > upperBound)
            {
                System.out.println("Пожалуйста, введите целое число от " + 1 + " до " + upperBound + " включительно:");
            }
        }

        return number;
    }
}