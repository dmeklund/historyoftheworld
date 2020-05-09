from xml.etree import ElementTree

class WikiParseApp:
    def main(self):
        self.parsewiki("/mnt/data/wiki/enwiki-20200401-pages-articles-multistream.xml")

    def parsewiki(self, filepath):
        with open(filepath, 'r') as f:
            context = ElementTree.iterparse(f) #, events=("start", "end"))
            # is_first = True
            # import ipdb; ipdb.set_trace()
            for event, elem in context:
                # print(event, elem)
                # if is_first:
                #     root = elem
                #     is_first = False
                if elem.tag.endswith("title"):
                    title = elem.text
                if elem.tag.endswith("text"):
                    text = elem.text
                    if "\n| coord" in text and "\n| date " in text:
                        print(title)
                elem.clear()


if __name__ == "__main__":
    WikiParseApp().main()
