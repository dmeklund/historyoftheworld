from pyspark.sql import SparkSession, SQLContext


def main():
    spark = SparkSession.builder.appName("hotw").getOrCreate()
    path = '/mnt/data/wiki/articles_in_xml_small_indented.xml'
    spark.read.format('xml').options(rowTag='page').load(path)


if __name__ == '__main__':
    main()
