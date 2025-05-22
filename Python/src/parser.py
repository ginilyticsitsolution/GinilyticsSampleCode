def parse_services(soup):
    services = []
    service_blocks = soup.select(".service-box")  # Replace with actual class or tag

    for block in service_blocks:
        title = block.find("h3").text.strip()
        desc = block.find("p").text.strip()
        services.append({
            "title": title,
            "description": desc
        })

    return services
