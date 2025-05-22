from src.scraper import fetch_page
from src.parser import parse_services
from src.writer import save_to_csv

def main():
    url = "https://ginilytics.com/portfolio/"  # Use actual services page
    soup = fetch_page(url)
    
    if soup:
        services = parse_services(soup)
        save_to_csv(services)

if __name__ == "__main__":
    main()
