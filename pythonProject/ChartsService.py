import datetime

import matplotlib
import matplotlib.pyplot as plt
import pylab

from matplotlib import dates


class ChartService:

    computer_name = ""
    collection = []

    def __init__(self, collection, computer_name):
        self.collection = collection
        self.computer_name = computer_name

    def generate_first_chart(self, running_time, power_on_count, date):
        x_values = [datetime.datetime.strptime(d, "%d/%m/%Y").date() for d in date]

        ax = plt.gca()
        ax2 = ax.twinx()

        formatter = dates.DateFormatter("%d/%m/%Y")

        ax.xaxis.set_major_formatter(formatter)

        locator = dates.DayLocator(interval=4)

        ax.xaxis.set_major_locator(locator)

        ax.set_ylabel("Running time in minutes", fontsize=14, color='red')
        ax2.set_ylabel("Power on count", fontsize=14, color='blue')

        ax.grid(True)

        ax.plot(x_values, running_time, "r-")
        ax2.plot(x_values, power_on_count)

        plt.gcf().autofmt_xdate()

        plt.title(self.computer_name + " From " + date[0] + " To " + date[-1])

        plt.savefig('BaseChart.jpg', dpi=300)

        plt.show()

    def generate_second_chart(self, gigabytes_sent, gigabytes_received, date):
        x_values = [datetime.datetime.strptime(d, "%d/%m/%Y").date() for d in date]

        ax = plt.gca()
        ax2 = ax.twinx()

        formatter = dates.DateFormatter("%d/%m/%Y")

        ax.xaxis.set_major_formatter(formatter)

        locator = dates.DayLocator(interval=4)

        ax.xaxis.set_major_locator(locator)

        ax.set_ylabel("Gigabytes received", fontsize=14, color='red')
        ax2.set_ylabel("Gigabytes sent", fontsize=14, color='blue')

        ax.grid(True)

        ax.plot(x_values, gigabytes_received, "r-")
        ax2.plot(x_values, gigabytes_sent)

        plt.gcf().autofmt_xdate()

        plt.title(self.computer_name + " From " + date[0] + " To " + date[-1])

        plt.savefig('NetChart.jpg', dpi=300)

        plt.show()


    def generate_chart(self):
        date = []
        power_on_count = []
        running_time = []

        gigabytes_received = []
        gigabytes_sent = []

        for i in range(len(self.collection)):
            date.append(self.collection[i][0])
            running_time.append(int(self.collection[i][1]))
            power_on_count.append(int(self.collection[i][2]))
            gigabytes_received.append(float(self.collection[i][3]))
            gigabytes_sent.append(float(self.collection[i][4]))

        self.generate_first_chart(running_time, power_on_count, date)

        self.generate_second_chart(gigabytes_sent, gigabytes_received, date)
